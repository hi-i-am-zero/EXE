using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.Media;

public class MediaFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public MediaFileType FileType { get; set; }
    public string? PublicUrl { get; set; }
    public string? Folder { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime CreatedAt { get; set; }
}
