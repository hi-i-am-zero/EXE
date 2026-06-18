namespace AutoWork.Application.DTOs.Facebook;

public class FacebookAccountDto
{
    public Guid Id { get; set; }
    public string FacebookUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<FacebookPageDto> Pages { get; set; } = [];
}
