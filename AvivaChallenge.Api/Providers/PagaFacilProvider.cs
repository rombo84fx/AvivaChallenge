using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Providers;

public class PagaFacilProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PagaFacilProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Name => "PagaFacil";

    public bool SupportsPaymentMode(PaymentMode mode) =>
        mode is PaymentMode.Cash or PaymentMode.CreditCard;

    public decimal CalculateFee(decimal amount, PaymentMode mode) => mode switch
    {
        PaymentMode.Cash => 15m,
        PaymentMode.CreditCard => amount * 0.01m,
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

        var result = await response.Content.ReadFromJsonAsync<PagaFacilOrderEntity>(JsonOptions);
        return MapToProviderOrder(result!);
    }

    public async Task<List<ProviderOrder>> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync("/Order");
        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<PagaFacilOrderModel>>(JsonOptions);
        return results?.Select(MapToProviderOrder).ToList() ?? [];
    }

    public async Task<ProviderOrder?> GetOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.GetAsync($"/Order/{providerOrderId}");
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<PagaFacilOrderModel>(JsonOptions);
        return result is null ? null : MapToProviderOrder(result);
    }

    public async Task CancelOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.PutAsync($"/cancel?id={providerOrderId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task PayOrderAsync(string providerOrderId)
    {
        var response = await _httpClient.PutAsync($"/pay?id={providerOrderId}", null);
        response.EnsureSuccessStatusCode();
    }

    private static string MapPaymentMode(PaymentMode mode) => mode switch
    {
        PaymentMode.Cash => "Cash",
        PaymentMode.CreditCard => "Card",
        _ => "None"
    };

    private static ProviderOrder MapToProviderOrder(PagaFacilOrderEntity entity) => new()
    {
        ProviderOrderId = entity.OrderId.ToString(),
        Amount = (decimal)entity.Amount,
        Status = entity.Status,
        Fees = entity.Fees?.Select(f => new Fee { Name = f.Name ?? string.Empty, Amount = (decimal)f.Amount }).ToList() ?? [],
        Products = entity.Products?.Select(p => new Product { Name = p.Name ?? string.Empty, UnitPrice = (decimal)p.UnitPrice }).ToList() ?? [],
        CreatedDate = entity.CreatedDate
    };

    private static ProviderOrder MapToProviderOrder(PagaFacilOrderModel model) => new()
    {
        ProviderOrderId = model.OrderId.ToString(),
        Amount = (decimal)model.Amount,
        Status = model.Status,
        Fees = model.Fees?.Select(f => new Fee { Name = f.Name ?? string.Empty, Amount = (decimal)f.Amount }).ToList() ?? [],
        Products = model.Products?.Select(p => new Product { Name = p.Name ?? string.Empty, UnitPrice = (decimal)p.UnitPrice }).ToList() ?? []
    };
}

internal class PagaFacilOrderEntity
{
    public Guid OrderId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<PagaFacilFee>? Fees { get; set; }
    public List<PagaFacilProduct>? Products { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
}

internal class PagaFacilOrderModel
{
    public Guid OrderId { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<PagaFacilFee>? Fees { get; set; }
    public List<PagaFacilProduct>? Products { get; set; }
}

internal class PagaFacilFee
{
    public string? Name { get; set; }
    public double Amount { get; set; }
}

internal class PagaFacilProduct
{
    public string? Name { get; set; }
    public double UnitPrice { get; set; }
}
