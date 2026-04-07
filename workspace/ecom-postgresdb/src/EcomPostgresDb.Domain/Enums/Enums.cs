namespace EcomPostgresDb.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}

public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2,
    BankTransfer = 3,
    Crypto = 4
}

public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    OutOfStock = 2,
    Discontinued = 3
}

public enum AddressType
{
    Billing = 0,
    Shipping = 1
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}
