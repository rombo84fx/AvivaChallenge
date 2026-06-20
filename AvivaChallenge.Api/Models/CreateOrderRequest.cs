using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Models;

public class CreateOrderRequest
{
    public string PaymentMode { get; set; } = string.Empty;
    public List<ProductDto> Products { get; set; } = [];
}

public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
