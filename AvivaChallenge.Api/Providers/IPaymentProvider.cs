using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Providers;

public class ProviderOrder
{
    public string ProviderOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Fee> Fees { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public DateTime CreatedDate { get; set; }
}

public interface IPaymentProvider
{
    string Name { get; }
    bool SupportsPaymentMode(PaymentMode mode);
    decimal CalculateFee(decimal amount, PaymentMode mode);
    Task<ProviderOrder> CreateOrderAsync(PaymentMode mode, List<Product> products);
    Task<List<ProviderOrder>> GetOrdersAsync();
    Task<ProviderOrder?> GetOrderAsync(string providerOrderId);
    Task CancelOrderAsync(string providerOrderId);
    Task PayOrderAsync(string providerOrderId);
}
