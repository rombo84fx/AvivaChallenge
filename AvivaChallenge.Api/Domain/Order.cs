namespace AvivaChallenge.Api.Domain;

public class Order
{
    public int OrderId { get; set; }
    public string ProviderOrderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal Amount => Products?.Sum(p => p.UnitPrice) ?? 0;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMode PaymentMode { get; set; }
    public List<Product> Products { get; set; } = [];
    public List<Fee> Fees { get; set; } = [];
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
