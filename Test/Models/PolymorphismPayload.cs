using Engine.Serialization.Binary.Attributes;

namespace Test.Models;

[BinaryKnownType(typeof(CardPayment))]
[BinaryKnownType(typeof(CashPayment))]
public abstract class PaymentMethod
{
    public string Currency { get; set; } = "USD";
}

public sealed class CardPayment : PaymentMethod
{
    public string Last4 { get; set; } = string.Empty;
}

public sealed class CashPayment : PaymentMethod
{
    public decimal Received { get; set; }
}

public sealed class CryptoPayment : PaymentMethod
{
    public string Wallet { get; set; } = string.Empty;
}

public sealed class OrderWithPolymorphism
{
    public int Id { get; set; }
    public PaymentMethod Payment { get; set; } = new CashPayment();
}