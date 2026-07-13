using System.Text.Json;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/workspace")]
public class WorkspaceController : ApiControllerBase
{
    private const string WorkspacePrefix = "Workspace:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public WorkspaceController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<List<WorkspaceProjectDto>>>> GetProjects()
    {
        var userId = GetUserId();
        var allProjects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var workspaceSettings = await _context.Settings
            .AsNoTracking()
            .Where(s => s.Key.StartsWith(WorkspacePrefix))
            .ToListAsync();

        var states = workspaceSettings
            .Select(s => new { s.Key, State = DeserializeState(s.Value) })
            .ToDictionary(x => x.Key, x => x.State);

        var projects = new List<WorkspaceProjectDto>();
        foreach (var project in allProjects)
        {
            var state = GetStateFromCache(states, project.Id, project.UserId);
            var isMember = state.Members.Any(m => m.UserId == userId && m.IsActive);
            if (project.UserId != userId && !isMember)
            {
                continue;
            }

            projects.Add(new WorkspaceProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsOwner = project.UserId == userId,
                MemberCount = state.Members.Count(m => m.IsActive),
                TotalTasks = state.Tasks.Count,
                PendingTasks = state.Tasks.Count(t => !string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase))
            });
        }

        return OkResponse(projects);
    }

    [HttpPost("projects")]
    public async Task<ActionResult<ApiResponse<WorkspaceProjectDto>>> CreateProject([FromBody] CreateWorkspaceProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return FailResponse<WorkspaceProjectDto>("Project name is required.");
        }

        var userId = GetUserId();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true
        };

        await _context.Projects.AddAsync(project);
        await SaveWorkspaceStateAsync(project.Id, new WorkspaceState
        {
            Members =
            [
                new WorkspaceMemberState
                {
                    UserId = userId,
                    Role = "Owner",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                }
            ]
        });
        await _context.SaveChangesAsync();

        return OkResponse(new WorkspaceProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            IsOwner = true,
            MemberCount = 1,
            TotalTasks = 0,
            PendingTasks = 0
        }, "Workspace project created.");
    }

    [HttpGet("projects/{projectId:guid}/overview")]
    public async Task<ActionResult<ApiResponse<WorkspaceOverviewDto>>> GetOverview(Guid projectId)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        var memberInfos = await GetMemberInfosAsync(state.Members);

        var dto = new WorkspaceOverviewDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Description = project.Description,
            IsOwner = project.UserId == userId,
            Members = memberInfos,
            Tasks = state.Tasks
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ToList(),
            TotalTasks = state.Tasks.Count,
            InProgressTasks = state.Tasks.Count(t => string.Equals(t.Status, "InProgress", StringComparison.OrdinalIgnoreCase)),
            CompletedTasks = state.Tasks.Count(t => string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase)),
            TodoTasks = state.Tasks.Count(t => string.Equals(t.Status, "Todo", StringComparison.OrdinalIgnoreCase))
        };

        return OkResponse(dto);
    }

    [HttpGet("projects/{projectId:guid}/members")]
    public async Task<ActionResult<ApiResponse<List<WorkspaceMemberDto>>>> GetMembers(Guid projectId)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        var members = await GetMemberInfosAsync(state.Members);
        return OkResponse(members);
    }

    [HttpPost("projects/{projectId:guid}/members")]
    public async Task<ActionResult<ApiResponse<WorkspaceMemberDto>>> AddMember(Guid projectId, [FromBody] AddWorkspaceMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return FailResponse<WorkspaceMemberDto>("Email is required.");
        }

        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        if (project.UserId != userId)
        {
            return FailResponse<WorkspaceMemberDto>("Only project owner can manage members.");
        }

        var memberUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());
        if (memberUser is null)
        {
            return FailResponse<WorkspaceMemberDto>("User email not found.");
        }

        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        var existing = state.Members.FirstOrDefault(m => m.UserId == memberUser.Id);
        if (existing is null)
        {
            state.Members.Add(new WorkspaceMemberState
            {
                UserId = memberUser.Id,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "Editor" : request.Role.Trim(),
                IsActive = true,
                AddedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Role = string.IsNullOrWhiteSpace(request.Role) ? existing.Role : request.Role.Trim();
            existing.IsActive = true;
        }

        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();

        return OkResponse(new WorkspaceMemberDto
        {
            UserId = memberUser.Id,
            Email = memberUser.Email,
            FullName = $"{memberUser.FirstName} {memberUser.LastName}".Trim(),
            Role = state.Members.First(m => m.UserId == memberUser.Id).Role,
            IsActive = true
        }, "Member added.");
    }

    [HttpDelete("projects/{projectId:guid}/members/{memberUserId:guid}")]
    public async Task<ActionResult<ApiResponse>> RemoveMember(Guid projectId, Guid memberUserId)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        if (project.UserId != userId)
        {
            return BadRequest(ApiResponse.Fail("Only project owner can remove members."));
        }

        if (memberUserId == project.UserId)
        {
            return BadRequest(ApiResponse.Fail("Owner cannot be removed."));
        }

        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        var member = state.Members.FirstOrDefault(m => m.UserId == memberUserId);
        if (member is null)
        {
            return BadRequest(ApiResponse.Fail("Member not found."));
        }

        member.IsActive = false;
        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse("Member removed.");
    }

    [HttpGet("projects/{projectId:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<List<WorkspaceTaskState>>>> GetTasks(Guid projectId)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
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
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        if (!CanEditWorkspace(state, project.UserId, userId))
        {
            return FailResponse<WorkspaceTaskState>("No permission to modify tasks.");
        }

        if (request.AssignedToUserId.HasValue &&
            !IsAssignableUser(state, project.UserId, request.AssignedToUserId.Value))
        {
            return FailResponse<WorkspaceTaskState>("Assigned user is not in this workspace.");
        }

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

    [HttpPut("projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse<WorkspaceTaskState>>> UpdateTask(
        Guid projectId,
        Guid taskId,
        [FromBody] UpdateWorkspaceTaskRequest request)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        if (!CanEditWorkspace(state, project.UserId, userId))
        {
            return FailResponse<WorkspaceTaskState>("No permission to modify tasks.");
        }

        if (request.AssignedToUserId.HasValue &&
            !IsAssignableUser(state, project.UserId, request.AssignedToUserId.Value))
        {
            return FailResponse<WorkspaceTaskState>("Assigned user is not in this workspace.");
        }

        var task = state.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            return FailResponse<WorkspaceTaskState>("Task not found.");
        }

        task.Title = string.IsNullOrWhiteSpace(request.Title) ? task.Title : request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = string.IsNullOrWhiteSpace(request.Priority) ? task.Priority : request.Priority.Trim();
        task.DueDate = request.DueDate;
        task.AssignedToUserId = request.AssignedToUserId;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            task.Status = request.Status.Trim();
        }

        if (string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase))
        {
            task.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse(task, "Task updated.");
    }

    [HttpPut("projects/{projectId:guid}/tasks/{taskId:guid}/status")]
    public async Task<ActionResult<ApiResponse<WorkspaceTaskState>>> UpdateTaskStatus(
        Guid projectId,
        Guid taskId,
        [FromBody] UpdateWorkspaceTaskStatusRequest request)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        if (!CanEditWorkspace(state, project.UserId, userId))
        {
            return FailResponse<WorkspaceTaskState>("No permission to modify tasks.");
        }

        var task = state.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            return FailResponse<WorkspaceTaskState>("Task not found.");
        }

        task.Status = string.IsNullOrWhiteSpace(request.Status) ? task.Status : request.Status.Trim();
        task.UpdatedAt = DateTime.UtcNow;
        task.CompletedAt = string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase)
            ? DateTime.UtcNow
            : null;

        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse(task, "Task status updated.");
    }

    [HttpDelete("projects/{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteTask(Guid projectId, Guid taskId)
    {
        var userId = GetUserId();
        var project = await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        if (!CanEditWorkspace(state, project.UserId, userId))
        {
            return BadRequest(ApiResponse.Fail("No permission to modify tasks."));
        }

        var removed = state.Tasks.RemoveAll(t => t.Id == taskId);
        if (removed == 0)
        {
            return BadRequest(ApiResponse.Fail("Task not found."));
        }

        await SaveWorkspaceStateAsync(project.Id, state);
        await _context.SaveChangesAsync();
        return OkResponse("Task removed.");
    }

    private Guid GetUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException();

    private static bool CanEditWorkspace(WorkspaceState state, Guid ownerUserId, Guid userId)
    {
        if (ownerUserId == userId)
        {
            return true;
        }

        var member = state.Members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
        if (member is null)
        {
            return false;
        }

        return !string.Equals(member.Role, "Viewer", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAssignableUser(WorkspaceState state, Guid ownerUserId, Guid userId)
    {
        if (ownerUserId == userId)
        {
            return true;
        }

        return state.Members.Any(m => m.UserId == userId && m.IsActive);
    }

    private async Task<Project> EnsureProjectAccessAsync(Guid projectId, Guid userId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.IsActive)
            ?? throw new ApplicationException("Project not found.");

        if (project.UserId == userId)
        {
            return project;
        }

        var state = await LoadWorkspaceStateAsync(project.Id, project.UserId);
        if (!state.Members.Any(m => m.UserId == userId && m.IsActive))
        {
            throw new UnauthorizedAccessException();
        }

        return project;
    }

    private async Task<WorkspaceState> LoadWorkspaceStateAsync(Guid projectId, Guid ownerUserId)
    {
        var key = WorkspaceKey(projectId);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return new WorkspaceState
            {
                Members =
                [
                    new WorkspaceMemberState
                    {
                        UserId = ownerUserId,
                        Role = "Owner",
                        IsActive = true,
                        AddedAt = DateTime.UtcNow
                    }
                ]
            };
        }

        var state = DeserializeState(setting.Value);
        if (!state.Members.Any(m => m.UserId == ownerUserId))
        {
            state.Members.Add(new WorkspaceMemberState
            {
                UserId = ownerUserId,
                Role = "Owner",
                IsActive = true,
                AddedAt = DateTime.UtcNow
            });
        }

        return state;
    }

    private async Task SaveWorkspaceStateAsync(Guid projectId, WorkspaceState state)
    {
        var key = WorkspaceKey(projectId);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        var value = JsonSerializer.Serialize(state, JsonOptions);

        if (setting is null)
        {
            await _context.Settings.AddAsync(new Setting
            {
                Key = key,
                Category = "Workspace",
                Description = $"Workspace state for project {projectId}",
                IsPublic = false,
                Value = value
            });
        }
        else
        {
            setting.Value = value;
            setting.Category = "Workspace";
            setting.Description = $"Workspace state for project {projectId}";
            setting.IsPublic = false;
        }
    }

    private async Task<List<WorkspaceMemberDto>> GetMemberInfosAsync(List<WorkspaceMemberState> members)
    {
        var ids = members.Where(m => m.IsActive).Select(m => m.UserId).Distinct().ToList();
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                users.TryGetValue(m.UserId, out var user);
                return new WorkspaceMemberDto
                {
                    UserId = m.UserId,
                    Role = m.Role,
                    IsActive = m.IsActive,
                    AddedAt = m.AddedAt,
                    Email = user?.Email ?? "unknown",
                    FullName = user is null ? "Unknown user" : $"{user.FirstName} {user.LastName}".Trim()
                };
            })
            .OrderByDescending(m => string.Equals(m.Role, "Owner", StringComparison.OrdinalIgnoreCase))
            .ThenBy(m => m.FullName)
            .ToList();
    }

    private static WorkspaceState GetStateFromCache(Dictionary<string, WorkspaceState> cache, Guid projectId, Guid ownerUserId)
    {
        var key = WorkspaceKey(projectId);
        if (cache.TryGetValue(key, out var state))
        {
            if (!state.Members.Any(m => m.UserId == ownerUserId))
            {
                state.Members.Add(new WorkspaceMemberState
                {
                    UserId = ownerUserId,
                    Role = "Owner",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                });
            }

            return state;
        }

        return new WorkspaceState
        {
            Members =
            [
                new WorkspaceMemberState
                {
                    UserId = ownerUserId,
                    Role = "Owner",
                    IsActive = true,
                    AddedAt = DateTime.UtcNow
                }
            ]
        };
    }

    private static WorkspaceState DeserializeState(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkspaceState>(json, JsonOptions) ?? new WorkspaceState();
        }
        catch
        {
            return new WorkspaceState();
        }
    }

    private static string WorkspaceKey(Guid projectId) => $"{WorkspacePrefix}{projectId}";

    public sealed class CreateWorkspaceProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public sealed class AddWorkspaceMemberRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";
    }

    public class CreateWorkspaceTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? AssignedToUserId { get; set; }
    }

    public class UpdateWorkspaceTaskRequest : CreateWorkspaceTaskRequest { }

    public sealed class UpdateWorkspaceTaskStatusRequest
    {
        public string Status { get; set; } = "Todo";
    }

    public sealed class WorkspaceProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsOwner { get; set; }
        public int MemberCount { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
    }

    public sealed class WorkspaceOverviewDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsOwner { get; set; }
        public int TotalTasks { get; set; }
        public int TodoTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public List<WorkspaceMemberDto> Members { get; set; } = [];
        public List<WorkspaceTaskState> Tasks { get; set; } = [];
    }

    public sealed class WorkspaceMemberDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Editor";
        public bool IsActive { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public sealed class WorkspaceState
    {
        public List<WorkspaceMemberState> Members { get; set; } = [];
        public List<WorkspaceTaskState> Tasks { get; set; } = [];
    }

    public sealed class WorkspaceMemberState
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Editor";
        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
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
