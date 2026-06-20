using System.Text.Json;
using System.Text.Json.Serialization;
using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Providers;

public class CazaPagosProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CazaPagosProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name => "CazaPagos";

    public bool SupportsPaymentMode(PaymentMode mode) =>
        mode is PaymentMode.CreditCard or PaymentMode.Transfer;

    public decimal CalculateFee(decimal amount, PaymentMode mode) => mode switch
    {
        PaymentMode.CreditCard => amount switch
        {
            <= 1500 => amount * 0.02m,
            <= 5000 => amount * 0.015m,
            _ => amount * 0.005m
        },
        PaymentMode.Transfer => amount switch
        {
            <= 500 => 5m,
            <= 1000 => amount * 0.025m,
            _ => amount * 0.02m
        },
        _ => decimal.MaxValue
    };

    public async Task<ProviderOrder> CreateOrderAsync(PaymentMode mode, List<Product> products)
    {
        var payload = new
        {
            Method = MapPaymentMode(mode),
            Products = products.Select(p => new { p.Name, p.UnitPrice }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("/Order", payload, JsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CazaPagosOrderEntity>(JsonOptions);
        return MapToProviderOrder(result!);
    }

    public async Task<List<ProviderOrder>> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync("/Order");
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<CazaPagosOrderModel>>(JsonOptions);
        return results?.Select(MapToProviderOrder).ToList() ?? [];
    }

    public async Task<ProviderOrder?> GetOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.GetAsync($"/Order/{providerOrderId}");
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<CazaPagosOrderModel>(JsonOptions);
        return result is null ? null : MapToProviderOrder(result);
    }

    public async Task CancelOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.PutAsync($"/cancellation?id={providerOrderId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task PayOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.PutAsync($"/payment?id={providerOrderId}", null);
        response.EnsureSuccessStatusCode();
    }

    private static string MapPaymentMode(PaymentMode mode) => mode switch
    {
        PaymentMode.CreditCard => "CreditCard",
        PaymentMode.Transfer => "Transfer",
        _ => "None"
    };

    private static ProviderOrder MapToProviderOrder(CazaPagosOrderEntity entity) => new()
    {
        ProviderOrderId = entity.OrderId ?? string.Empty,
        Amount = (decimal)entity.Amount,
        Status = entity.Status,
        Fees = entity.Fees?.Select(f => new Fee { Name = f.Title ?? string.Empty, Amount = (decimal)f.Amount }).ToList() ?? [],
        Products = entity.Products?.Select(p => new Product { Name = p.Name ?? string.Empty, UnitPrice = (decimal)p.UnitPrice }).ToList() ?? [],
        CreatedDate = entity.CreatedDate
    };

    private static ProviderOrder MapToProviderOrder(CazaPagosOrderModel model) => new()
    {
        ProviderOrderId = model.OrderId ?? string.Empty,
        Amount = (decimal)model.Amount,
        Status = model.Status,
        Fees = model.Fees?.Select(f => new Fee { Name = f.Title ?? string.Empty, Amount = (decimal)f.Amount }).ToList() ?? [],
        Products = model.Products?.Select(p => new Product { Name = p.Name ?? string.Empty, UnitPrice = (decimal)p.UnitPrice }).ToList() ?? []
    };
}

internal class CazaPagosOrderEntity
{
    public string? OrderId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<CazaPagosFee>? Fees { get; set; }
    public List<CazaPagosTax>? Taxes { get; set; }
    public List<CazaPagosProduct>? Products { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}

internal class CazaPagosOrderModel
{
    public string? OrderId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<CazaPagosFee>? Fees { get; set; }
    public List<CazaPagosTax>? Taxes { get; set; }
    public List<CazaPagosProduct>? Products { get; set; }
}

internal class CazaPagosFee
{
    public string? Title { get; set; }
    public double Amount { get; set; }
}

internal class CazaPagosTax
{
    public string? Tax { get; set; }
    public double Amount { get; set; }
}

internal class CazaPagosProduct
{
    public string? Name { get; set; }
    public double UnitPrice { get; set; }
}
