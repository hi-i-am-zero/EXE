namespace AutoWork.Infrastructure.Settings;

public class WordPressSettings
{
    public const string SectionName = "WordPressSettings";

    public int RequestTimeoutSeconds { get; set; } = 60;

    public string DefaultPostStatus { get; set; } = "draft";
}
