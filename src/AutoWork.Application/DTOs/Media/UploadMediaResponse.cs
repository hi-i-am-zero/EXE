namespace AutoWork.Application.DTOs.Media;

public class UploadMediaResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
