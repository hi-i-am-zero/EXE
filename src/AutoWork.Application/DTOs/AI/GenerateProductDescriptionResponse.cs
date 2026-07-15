namespace AutoWork.Application.DTOs.AI;

public class GenerateProductDescriptionResponse
{
    public Guid ProductId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int CreditsUsed { get; set; }

    public int TokensUsed { get; set; }
}
