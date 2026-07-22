namespace AutoWork.Application.Common.Helpers;

public static class PhoneHelper
{
    public static string? Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84") && digits.Length == 11)
            return "0" + digits[2..];

        return digits;
    }

    public static bool IsValid(string? phone)
    {
        var normalized = Normalize(phone);
        return normalized is { Length: 10 } && normalized.StartsWith('0');
    }
}
