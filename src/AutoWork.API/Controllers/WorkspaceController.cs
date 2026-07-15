using System.Text.Json;
using AutoWork.Application.DTOs.Workspace;
using AutoWork.Application.Features.Workspace.Commands;
using AutoWork.Application.Features.Workspace.Queries;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.API.Controllers;

/// <summary>
/// FlowMate v2: phần Members/Projects đã chuyển sang CQRS thật (MediatR + Repository&lt;ProjectMember&gt;),
/// thay cho cách lưu JSON vào bảng Settings trước đây.
///
/// Phần Tasks (Todo/InProgress/Done ở tab "Nhóm của tôi") VẪN dùng cơ chế JSON-in-Settings cũ —
/// việc này nằm ngoài phạm vi 4 bước Phase 2 đã thống nhất (chưa có Task entity chuẩn hoá).
/// Giữ nguyên để không phá vỡ tính năng đang chạy; sẽ chuẩn hoá thành entity riêng khi có yêu cầu.
/// </summary>
[Authorize]
[Route("api/workspace")]
public class WorkspaceController : ApiControllerBase
{
    private const string WorkspacePrefix = "Workspace:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public WorkspaceController(IMediator mediator, ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _context = context;
        _currentUser = currentUser;
    }

    // ===================== Projects (CQRS) =====================

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetProjects()
    {
        var result = await _mediator.Send(new GetProjectsQuery());
        return OkResponse(result);
    }

    [HttpPost("projects")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectDto request) =>
        OkResponse(await _mediator.Send(new CreateProjectCommand { Request = request }), "Workspace created.");

    // ===================== Members (CQRS) =====================

    [HttpGet("projects/{projectId:guid}/members")]
    public async Task<ActionResult<ApiResponse<List<ProjectMemberDto>>>> GetMembers(Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectMembersQuery { ProjectId = projectId });
        return OkResponse(result);
    }

    [HttpPost("projects/{projectId:guid}/members")]
    public async Task<ActionResult<ApiResponse<ProjectMemberDto>>> AddMember(
        Guid projectId, [FromBody] AddProjectMemberDto request)
    {
        request.ProjectId = projectId;
        var result = await _mediator.Send(new AddProjectMemberCommand { Request = request });
        return OkResponse(result, "Member added.");
    }

    [HttpPut("projects/{projectId:guid}/members/{memberUserId:guid}/role")]
    public async Task<ActionResult<ApiResponse>> UpdateMemberRole(
        Guid projectId, Guid memberUserId, [FromBody] UpdateProjectMemberRoleDto request)
    {
        request.ProjectId = projectId;
        request.UserId = memberUserId;
        await _mediator.Send(new UpdateProjectMemberRoleCommand { Request = request });
        return OkResponse("Role updated.");
    }

    [HttpDelete("projects/{projectId:guid}/members/{memberUserId:guid}")]
    public async Task<ActionResult<ApiResponse>> RemoveMember(Guid projectId, Guid memberUserId)
    {
        await _mediator.Send(new RemoveProjectMemberCommand { ProjectId = projectId, MemberUserId = memberUserId });
        return OkResponse("Member removed.");
    }

    /// <summary>Gộp Members (CQRS) + Tasks (legacy JSON-in-Settings) + thống kê vào 1 response
    /// duy nhất — đúng theo hợp đồng mà workspace.html (frontend) đang gọi.</summary>
    [HttpGet("projects/{projectId:guid}/overview")]
    public async Task<ActionResult<ApiResponse<object>>> GetOverview(Guid projectId)
    {
        var userId = GetUserId();
        var project = await EnsureLegacyProjectAccessAsync(projectId, userId);

        var members = await _mediator.Send(new GetProjectMembersQuery { ProjectId = projectId });
        var state = await LoadWorkspaceStateAsync(projectId);
        var tasks = state.Tasks;

        var overview = new
        {
            isOwner = project.UserId == userId,
            members,
            tasks,
            totalTasks = tasks.Count,
            todoTasks = tasks.Count(t => string.Equals(t.Status, "Todo", StringComparison.OrdinalIgnoreCase)),
            inProgressTasks = tasks.Count(t => string.Equals(t.Status, "InProgress", StringComparison.OrdinalIgnoreCase)),
            completedTasks = tasks.Count(t => string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase))
        };

        return OkResponse<object>(overview);
    }

    // ===================== Tasks (LEGACY — giữ nguyên JSON-in-Settings, ngoài phạm vi Phase 2) =====================

    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<List<WorkspaceTaskState>>>> GetTasks(Guid projectId)
    {
        var userId = GetUserId();
        var project = await EnsureLegacyProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id);
        return OkResponse(state.Tasks
            .OrderBy(t => string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .ThenBy(t => t.DueDate)
            .ToList());
    }

    [HttpPost("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<WorkspaceTaskState>>> CreateTask(Guid projectId, [FromBody] CreateWorkspaceTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return FailResponse<WorkspaceTaskState>("Task title is required.");
        }

        var userId = GetUserId();
        var project = await EnsureLegacyProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id);

        var task = new WorkspaceTaskState
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Todo" : request.Status.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
            DueDate = request.DueDate,
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = DateTime.UtcNow
        };

        state.Tasks.Add(task);
        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse(task, "Task created.");
    }

    [HttpPut("projects/{projectId:guid}/tasks/{taskId:guid}/status")]
    public async Task<ActionResult<ApiResponse<WorkspaceTaskState>>> UpdateTaskStatus(
        Guid projectId, Guid taskId, [FromBody] UpdateWorkspaceTaskStatusRequest request)
    {
        var userId = GetUserId();
        var project = await EnsureLegacyProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id);

        var task = state.Tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new Application.Common.Exceptions.NotFoundException("Task", taskId);

        task.Status = string.IsNullOrWhiteSpace(request.Status) ? task.Status : request.Status.Trim();
        task.UpdatedAt = DateTime.UtcNow;
        task.CompletedAt = string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null;

        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse(task, "Task status updated.");
    }

    [HttpDelete("projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteTask(Guid projectId, Guid taskId)
    {
        var userId = GetUserId();
        var project = await EnsureLegacyProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id);

        if (state.Tasks.RemoveAll(t => t.Id == taskId) == 0)
        {
            return BadRequest(ApiResponse.Fail("Task not found."));
        }

        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse("Task removed.");
    }

    private Guid GetUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException();

    private async Task<Project> EnsureLegacyProjectAccessAsync(Guid projectId, Guid userId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.IsActive)
            ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Project), projectId);

        if (project.UserId == userId)
        {
            return project;
        }

        var isMember = await _context.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException();
        }

        return project;
    }

    private async Task<WorkspaceState> LoadWorkspaceStateAsync(Guid projectId)
    {
        var key = WorkspaceKey(projectId);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return new WorkspaceState();
        }

        try
        {
            return JsonSerializer.Deserialize<WorkspaceState>(setting.Value, JsonOptions) ?? new WorkspaceState();
        }
        catch
        {
            return new WorkspaceState();
        }
    }

    private async Task SaveWorkspaceStateAsync(Guid projectId, WorkspaceState state)
    {
        var key = WorkspaceKey(projectId);
        var value = JsonSerializer.Serialize(state, JsonOptions);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);

        if (setting is null)
        {
            await _context.Settings.AddAsync(new Setting
            {
                Key = key,
                Category = "Workspace.Tasks",
                Description = $"Legacy task list for project {projectId}",
                IsPublic = false,
                Value = value
            });
        }
        else
        {
            setting.Value = value;
        }
    }

    private static string WorkspaceKey(Guid projectId) => $"{WorkspacePrefix}{projectId}";

    public class CreateWorkspaceTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToUserId { get; set; }
    }

    public sealed class UpdateWorkspaceTaskStatusRequest
    {
        public string Status { get; set; } = "Todo";
    }

    public sealed class WorkspaceState
    {
        public List<WorkspaceTaskState> Tasks { get; set; } = [];
    }

    public sealed class WorkspaceTaskState
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Todo";
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
