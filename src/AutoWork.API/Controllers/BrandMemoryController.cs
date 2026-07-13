using System.Text.Json;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Persistence.Context;
using AutoWork.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoWork.API.Controllers;

[Authorize]
[Route("api/brand-memory")]
public class BrandMemoryController : ApiControllerBase
{
    private const string BrandMemoryPrefix = "BrandMemory:";
    private const string WorkspacePrefix = "Workspace:";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public BrandMemoryController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<BrandMemoryResponse>>> Get(Guid projectId)
    {
        var userId = GetUserId();
        await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadBrandMemoryStateAsync(projectId);
        var response = BuildResponse(projectId, state);
        return OkResponse(response);
    }

    [HttpPut("projects/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<BrandMemoryResponse>>> Upsert(
        Guid projectId,
        [FromBody] BrandMemoryState request)
    {
        var userId = GetUserId();
        await EnsureProjectAccessAsync(projectId, userId);
        var state = request ?? new BrandMemoryState();
        state.UpdatedAt = DateTime.UtcNow;

        await SaveBrandMemoryStateAsync(projectId, state);
        await _context.SaveChangesAsync();

        return OkResponse(BuildResponse(projectId, state), "Brand Memory saved.");
    }

    [HttpGet("projects/{projectId:guid}/summary")]
    public async Task<ActionResult<ApiResponse<BrandMemorySummaryResponse>>> GetSummary(Guid projectId)
    {
        var userId = GetUserId();
        await EnsureProjectAccessAsync(projectId, userId);
        var state = await LoadBrandMemoryStateAsync(projectId);
        var sections = CalculateSections(state);

        return OkResponse(new BrandMemorySummaryResponse
        {
            ProjectId = projectId,
            CompletedSections = sections.Completed,
            TotalSections = sections.Total,
            CompletionPercent = sections.Percent,
            Tips = BuildTips(state),
            Keywords = state.BrandKeywords,
            Hashtags = state.Hashtags,
            CtaPhrases = state.CtaPhrases
        });
    }

    private Guid GetUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException();

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.IsActive);
        if (project is null)
        {
            throw new ApplicationException("Project not found.");
        }

        if (project.UserId == userId)
        {
            return;
        }

        var key = $"{WorkspacePrefix}{projectId}";
        var workspace = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        if (workspace is null || string.IsNullOrWhiteSpace(workspace.Value))
        {
            throw new UnauthorizedAccessException();
        }

        var state = DeserializeWorkspace(workspace.Value);
        if (!state.Members.Any(m => m.UserId == userId && m.IsActive))
        {
            throw new UnauthorizedAccessException();
        }
    }

    private async Task<BrandMemoryState> LoadBrandMemoryStateAsync(Guid projectId)
    {
        var key = BrandMemoryKey(projectId);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return new BrandMemoryState();
        }

        try
        {
            return JsonSerializer.Deserialize<BrandMemoryState>(setting.Value, JsonOptions) ?? new BrandMemoryState();
        }
        catch
        {
            return new BrandMemoryState();
        }
    }

    private async Task SaveBrandMemoryStateAsync(Guid projectId, BrandMemoryState state)
    {
        var key = BrandMemoryKey(projectId);
        var json = JsonSerializer.Serialize(state, JsonOptions);
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            await _context.Settings.AddAsync(new Domain.Entities.Setting
            {
                Key = key,
                Category = "BrandMemory",
                Description = $"Brand Memory for project {projectId}",
                IsPublic = false,
                Value = json
            });
        }
        else
        {
            setting.Value = json;
            setting.Category = "BrandMemory";
            setting.Description = $"Brand Memory for project {projectId}";
            setting.IsPublic = false;
        }
    }

    private static string BrandMemoryKey(Guid projectId) => $"{BrandMemoryPrefix}{projectId}";

    private static BrandMemoryResponse BuildResponse(Guid projectId, BrandMemoryState state)
    {
        var sections = CalculateSections(state);
        return new BrandMemoryResponse
        {
            ProjectId = projectId,
            Data = state,
            CompletedSections = sections.Completed,
            TotalSections = sections.Total,
            CompletionPercent = sections.Percent
        };
    }

    private static (int Completed, int Total, int Percent) CalculateSections(BrandMemoryState state)
    {
        var checks = new[]
        {
            !string.IsNullOrWhiteSpace(state.BusinessName) || !string.IsNullOrWhiteSpace(state.BrandName),
            !string.IsNullOrWhiteSpace(state.Address) || !string.IsNullOrWhiteSpace(state.Email) || !string.IsNullOrWhiteSpace(state.Phone),
            state.SalesChannels.Count > 0,
            state.ProductCategories.Count > 0,
            state.BrandKeywords.Count > 0 || state.StyleTags.Count > 0 || !string.IsNullOrWhiteSpace(state.PrimaryColor)
        };

        var completed = checks.Count(c => c);
        var total = checks.Length;
        var percent = (int)Math.Round((double)completed / total * 100, MidpointRounding.AwayFromZero);
        return (completed, total, percent);
    }

    private static List<string> BuildTips(BrandMemoryState state)
    {
        var tips = new List<string>();
        if (string.IsNullOrWhiteSpace(state.ShortDescription))
        {
            tips.Add("Thêm mô tả ngắn thương hiệu để AI hiểu đúng định vị.");
        }
        if (state.BrandKeywords.Count < 3)
        {
            tips.Add("Bổ sung ít nhất 3 từ khóa thương hiệu.");
        }
        if (string.IsNullOrWhiteSpace(state.PrimaryColor))
        {
            tips.Add("Cập nhật màu chủ đạo để đồng bộ visual nội dung.");
        }
        if (state.CtaPhrases.Count == 0)
        {
            tips.Add("Thêm CTA hay dùng để AI tạo lời kêu gọi hành động nhất quán.");
        }

        if (tips.Count == 0)
        {
            tips.Add("Brand Memory đã khá đầy đủ. Bạn có thể tiếp tục tối ưu theo từng chiến dịch.");
        }

        return tips;
    }

    private static WorkspaceStateLite DeserializeWorkspace(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkspaceStateLite>(json, JsonOptions) ?? new WorkspaceStateLite();
        }
        catch
        {
            return new WorkspaceStateLite();
        }
    }

    public sealed class BrandMemoryResponse
    {
        public Guid ProjectId { get; set; }
        public BrandMemoryState Data { get; set; } = new();
        public int CompletedSections { get; set; }
        public int TotalSections { get; set; }
        public int CompletionPercent { get; set; }
    }

    public sealed class BrandMemorySummaryResponse
    {
        public Guid ProjectId { get; set; }
        public int CompletedSections { get; set; }
        public int TotalSections { get; set; }
        public int CompletionPercent { get; set; }
        public List<string> Tips { get; set; } = [];
        public List<string> Keywords { get; set; } = [];
        public List<string> Hashtags { get; set; } = [];
        public List<string> CtaPhrases { get; set; } = [];
    }

    public sealed class BrandMemoryState
    {
        public string? BusinessName { get; set; }
        public string? BrandName { get; set; }
        public string? Industry { get; set; }
        public int? EstablishedYear { get; set; }
        public string? ShortDescription { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? AccentColor { get; set; }
        public string? FontTitle { get; set; }
        public string? FontBody { get; set; }
        public List<string> SalesChannels { get; set; } = [];
        public List<string> ProductCategories { get; set; } = [];
        public List<string> StyleTags { get; set; } = [];
        public List<string> ToneSamples { get; set; } = [];
        public List<string> ToneKeywords { get; set; } = [];
        public List<string> BrandKeywords { get; set; } = [];
        public List<string> CtaPhrases { get; set; } = [];
        public List<string> Hashtags { get; set; } = [];
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class WorkspaceStateLite
    {
        public List<WorkspaceMemberLite> Members { get; set; } = [];
    }

    private sealed class WorkspaceMemberLite
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
