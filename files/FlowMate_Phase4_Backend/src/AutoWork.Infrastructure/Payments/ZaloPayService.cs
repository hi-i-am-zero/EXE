using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoWork.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoWork.Infrastructure.Payments;

public class ZaloPayService
{
    private readonly HttpClient _httpClient;
    private readonly ZaloPaySettings _settings;
    private readonly ILogger<ZaloPayService> _logger;

    public ZaloPayService(HttpClient httpClient, IOptions<PaymentSettings> settings, ILogger<ZaloPayService> logger)
    {
        _httpClient = httpClient; _settings = settings.Value.ZaloPay; _logger = logger;
    }

    public async Task<string> CreatePaymentUrlAsync(string appTransId, long amount, string description, string embedData, string item, string callbackUrl, CancellationToken cancellationToken = default)
    {
        var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var mac = ComputeHmacSha256($"{_settings.AppId}|{appTransId}|{amount}|{appTime}|{embedData}|{item}", _settings.Key1);
        var payload = new Dictionary<string, object> { ["app_id"] = _settings.AppId, ["app_trans_id"] = appTransId, ["app_user"] = "AutoWork", ["app_time"] = appTime, ["amount"] = amount, ["description"] = description, ["embed_data"] = embedData, ["item"] = item, ["callback_url"] = callbackUrl, ["mac"] = mac };
        using var response = await _httpClient.PostAsync(_settings.Endpoint, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode) { _logger.LogError("ZaloPay create payment failed: {Body}", body); throw new InvalidOperationException("Failed to create ZaloPay payment."); }
        return JsonSerializer.Deserialize<ZaloPayCreateResponse>(body)?.OrderUrl ?? throw new InvalidOperationException("ZaloPay URL missing.");
    }

    public bool ValidateCallback(IDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("mac", out var mac)) return false;
        var data = parameters.TryGetValue("data", out var payload) ? payload : string.Empty;
        return string.Equals(ComputeHmacSha256(data, _settings.Key2), mac, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private sealed class ZaloPayCreateResponse { public string? OrderUrl { get; set; } }
}
