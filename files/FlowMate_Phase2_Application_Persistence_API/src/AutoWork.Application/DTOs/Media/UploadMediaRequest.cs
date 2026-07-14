namespace AutoWork.Application.DTOs.Media;

public class UploadMediaRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Folder { get; set; }
    public Stream FileStream { get; set; } = Stream.Null;
}
