namespace AutoWork.Application.DTOs.Affiliates;

public class AffiliateLinkDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Campaign { get; set; }
    public int ClickCount { get; set; }
    public int ConversionCount { get; set; }
}

public class CreateAffiliateLinkRequest
{
    public string? Campaign { get; set; }
}
