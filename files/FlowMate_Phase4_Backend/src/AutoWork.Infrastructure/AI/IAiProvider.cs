using AutoWork.Shared.Enums;

namespace AutoWork.Infrastructure.AI;

public interface IAiProvider
{
    AiProvider Provider { get; }

    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken = default);
}

public class AiGenerationRequest
{
    public string SystemPrompt { get; set; } = string.Empty;

    public string UserPrompt { get; set; } = string.Empty;

    public int MaxTokens { get; set; } = 2048;

    /// <summary>Ảnh đính kèm cho yêu cầu có vision (VD: sinh mô tả sản phẩm từ ảnh).
    /// Dùng base64 data URI (vd: "data:image/jpeg;base64,...") để nhất quán giữa các provider —
    /// Claude bắt buộc base64, còn OpenAI/Gemini có thể nhận URL trực tiếp nhưng base64 an toàn
    /// hơn khi ảnh không public (VD: file mới upload chưa có CDN công khai).</summary>
    public List<string> ImageDataUris { get; set; } = [];
}

public class AiGenerationResult
{
    public string RawContent { get; set; } = string.Empty;

    public int TokensUsed { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? Hashtags { get; set; }
}
