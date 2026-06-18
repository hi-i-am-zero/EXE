namespace AutoWork.Application.DTOs.Zalo;

public class ZaloAccountDto
{
    public Guid Id { get; set; }
    public string OaId { get; set; } = string.Empty;
    public string OaName { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
