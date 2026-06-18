namespace AutoWork.Domain.Enums;

/// <summary>Method used to settle a payment.</summary>
public enum PaymentMethod
{
    CreditCard = 0,
    BankTransfer = 1,
    Momo = 2,
    VnPay = 3,
    PayPal = 4,
    Other = 99
}
