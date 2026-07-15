using System.Net;
using System.Security.Cryptography;
using System.Text;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Payments;

public class VNPayService
{
    private readonly VNPaySettings _settings;
    public VNPayService(IOptions<PaymentSettings> settings) => _settings = settings.Value.VNPay;

    public string CreatePaymentUrl(string transactionId, decimal amount, string orderInfo, string returnUrl, string ipAddress, DateTime createdDate)
    {
        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0", ["vnp_Command"] = "pay", ["vnp_TmnCode"] = _settings.TmnCode,
            ["vnp_Amount"] = ((long)(amount * 100)).ToString(), ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = transactionId, ["vnp_OrderInfo"] = orderInfo, ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn", ["vnp_ReturnUrl"] = returnUrl, ["vnp_IpAddr"] = ipAddress,
            ["vnp_CreateDate"] = createdDate.ToString("yyyyMMddHHmmss")
        };
        var query = string.Join("&", parameters.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
        return $"{_settings.PaymentUrl}?{query}&vnp_SecureHash={ComputeHash(query)}";
    }

    public bool ValidateCallback(IDictionary<string, string> parameters, out string transactionId, out string responseCode)
    {
        transactionId = parameters.TryGetValue("vnp_TxnRef", out var txn) ? txn : string.Empty;
        responseCode = parameters.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        if (!parameters.TryGetValue("vnp_SecureHash", out var secureHash)) return false;
        var filtered = parameters.Where(p => p.Key.StartsWith("vnp_", StringComparison.Ordinal) && p.Key != "vnp_SecureHash").OrderBy(p => p.Key, StringComparer.Ordinal);
        var query = string.Join("&", filtered.Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));
        return string.Equals(ComputeHash(query), secureHash, StringComparison.OrdinalIgnoreCase);
    }

    private string ComputeHash(string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.HashSecret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }
}
