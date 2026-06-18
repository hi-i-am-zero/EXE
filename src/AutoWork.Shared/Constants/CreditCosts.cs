using AutoWork.Shared.Enums;

namespace AutoWork.Shared.Constants;

public static class CreditCosts
{
    public const int GenerateContent = 10;
    public const int GenerateSeo = 8;
    public const int GenerateProductContent = 5;

    public static int GetCost(CreditTransactionType transactionType) =>
        transactionType switch
        {
            CreditTransactionType.GenerateContent => GenerateContent,
            CreditTransactionType.GenerateSeo => GenerateSeo,
            CreditTransactionType.GenerateProductContent => GenerateProductContent,
            _ => 0
        };

    public static int GetCost(AiContentType contentType) =>
        contentType switch
        {
            AiContentType.BlogPost or AiContentType.SocialPost or AiContentType.Email or AiContentType.AdCopy
                => GenerateContent,
            AiContentType.SeoArticle or AiContentType.Title or AiContentType.MetaDescription
                => GenerateSeo,
            AiContentType.ProductDescription or AiContentType.ProductShortDescription
                => GenerateProductContent,
            AiContentType.Hashtag => 2,
            _ => GenerateContent
        };
}
