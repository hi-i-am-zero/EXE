namespace AutoWork.Infrastructure.Settings;

public class PaymentSettings
{
    public const string SectionName = "PaymentSettings";

    public string DefaultReturnUrl { get; set; } = "https://localhost:5001/payments/return";

    public string DefaultCancelUrl { get; set; } = "https://localhost:5001/payments/cancel";

    public VNPaySettings VNPay { get; set; } = new();

    public MoMoSettings MoMo { get; set; } = new();

    public ZaloPaySettings ZaloPay { get; set; } = new();
}

public class VNPaySettings
{
    public string TmnCode { get; set; } = string.Empty;

    public string HashSecret { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    public string ApiUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
}

public class MoMoSettings
{
    public string PartnerCode { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
}

public class ZaloPaySettings
{
    public int AppId { get; set; }

    public string Key1 { get; set; } = string.Empty;

    public string Key2 { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://sb-openapi.zalopay.vn/v2/create";
}
