using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Payments;

public class MoMoService
{
    private readonly HttpClient _httpClient;
    private readonly MoMoSettings _settings;
    private readonly ILogger<MoMoService> _logger;

    public MoMoService(HttpClient httpClient, IOptions<PaymentSettings> settings, ILogger<MoMoService> logger)
    {
        _httpClient = httpClient; _settings = settings.Value.MoMo; _logger = logger;
    }

    public async Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string orderInfo, string returnUrl, string notifyUrl, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var rawSignature = $"accessKey={_settings.AccessKey}&amount={(long)amount}&extraData=&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_settings.PartnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
        var signature = ComputeHmacSha256(rawSignature, _settings.SecretKey);
        var payload = new { partnerCode = _settings.PartnerCode, accessKey = _settings.AccessKey, requestId, amount = (long)amount, orderId, orderInfo, redirectUrl = returnUrl, ipnUrl = notifyUrl, extraData = string.Empty, requestType = "captureWallet", signature, lang = "vi" };
        using var response = await _httpClient.PostAsync(_settings.Endpoint, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) { _logger.LogError("MoMo create payment failed: {Body}", body); throw new InvalidOperationException("Failed to create MoMo payment."); }
        return JsonSerializer.Deserialize<MoMoCreateResponse>(body)?.PayUrl ?? throw new InvalidOperationException("MoMo URL missing.");
    }

    public bool ValidateIpn(IDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("signature", out var signature)) return false;
        var raw = $"accessKey={Get(parameters, "accessKey")}&amount={Get(parameters, "amount")}&extraData={Get(parameters, "extraData")}&message={Get(parameters, "message")}&orderId={Get(parameters, "orderId")}&orderInfo={Get(parameters, "orderInfo")}&orderType={Get(parameters, "orderType")}&partnerCode={Get(parameters, "partnerCode")}&payType={Get(parameters, "payType")}&requestId={Get(parameters, "requestId")}&responseTime={Get(parameters, "responseTime")}&resultCode={Get(parameters, "resultCode")}&transId={Get(parameters, "transId")}";
        return string.Equals(ComputeHmacSha256(raw, _settings.SecretKey), signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private static string Get(IDictionary<string, string> data, string key) => data.TryGetValue(key, out var value) ? value : string.Empty;
    private sealed class MoMoCreateResponse { public string? PayUrl { get; set; } }
}
