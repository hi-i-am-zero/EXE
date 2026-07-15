using AutoWork.Shared.Enums;

namespace AutoWork.Application.DTOs.AI;

public class GenerateProductDescriptionRequest
{
    public Guid ProductId { get; set; }

    public AiProvider Provider { get; set; } = AiProvider.Claude;
}
