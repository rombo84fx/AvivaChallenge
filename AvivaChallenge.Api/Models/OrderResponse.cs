namespace AvivaChallenge.Api.Models;

public class OrderResponse
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderOrderId { get; set; } = string.Empty;
    public List<FeeDto> Fees { get; set; } = [];
    public List<ProductDto> Products { get; set; } = [];
    public DateTime CreatedDate { get; set; }
}

public class FeeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
