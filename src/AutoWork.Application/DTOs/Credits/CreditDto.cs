namespace AutoWork.Application.DTOs.Credits;

public class CreditDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Balance { get; set; }
    public int TotalEarned { get; set; }
    public int TotalUsed { get; set; }
}
