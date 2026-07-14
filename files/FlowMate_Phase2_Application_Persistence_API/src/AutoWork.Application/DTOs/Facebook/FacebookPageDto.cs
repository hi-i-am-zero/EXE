namespace AutoWork.Application.DTOs.Facebook;

public class FacebookPageDto
{
    public Guid Id { get; set; }
    public Guid FacebookAccountId { get; set; }
    public string PageId { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? PictureUrl { get; set; }
    public bool IsActive { get; set; }
}
