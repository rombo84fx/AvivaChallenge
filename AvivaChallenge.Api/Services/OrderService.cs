using AvivaChallenge.Api.Domain;
using AvivaChallenge.Api.Models;
using AvivaChallenge.Api.Providers;
using AvivaChallenge.Api.Repositories;

namespace AvivaChallenge.Api.Services;

public class OrderService : IOrderService
{
    private readonly IPaymentProviderSelector _providerSelector;
    private readonly IOrderRepository _orderRepository;

    public OrderService(IPaymentProviderSelector providerSelector, IOrderRepository orderRepository)
    {
        _providerSelector = providerSelector;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        if (!Enum.TryParse<PaymentMode>(request.PaymentMode, true, out var paymentMode))
            throw new ArgumentException($"Invalid payment mode: '{request.PaymentMode}'. Valid values: Cash, CreditCard, Transfer.");

        if (request.Products is not { Count: > 0 })
            throw new ArgumentException("At least one product is required.");

        var products = request.Products
            .Select(p => new Product { Name = p.Name, UnitPrice = p.UnitPrice })
            .ToList();

        var totalAmount = products.Sum(p => p.UnitPrice);
        var provider = _providerSelector.SelectOptimalProvider(totalAmount, paymentMode);

        var providerOrder = await provider.CreateOrderAsync(paymentMode, products);

        var order = new Order
        {
            ProviderOrderId = providerOrder.ProviderOrderId,
            ProviderName = provider.Name,
            Status = OrderStatus.Pending,
            PaymentMode = paymentMode,
            Products = providerOrder.Products,
            Fees = providerOrder.Fees,
            CreatedDate = providerOrder.CreatedDate
        };

        _orderRepository.Add(order);

        return MapToResponse(order);
    }

    public Task<List<OrderResponse>> GetOrdersAsync()
    {
        var orders = _orderRepository.GetAll();
        var responses = orders.Select(MapToResponse).ToList();
        return Task.FromResult(responses);
    }

    public Task<OrderResponse?> GetOrderAsync(int orderId)
    {
        var order = _orderRepository.GetById(orderId);
        return Task.FromResult(order is null ? null : MapToResponse(order));
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order is null) return false;

        var provider = GetProviderForOrder(order);
        await provider.CancelOrderAsync(order.ProviderOrderId);

        order.Status = OrderStatus.Cancelled;
        _orderRepository.Update(order);

        return true;
    }

    public async Task<bool> PayOrderAsync(int orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order is null) return false;

        var provider = GetProviderForOrder(order);
        await provider.PayOrderAsync(order.ProviderOrderId);

        order.Status = OrderStatus.Paid;
        _orderRepository.Update(order);

        return true;
    }

    private IPaymentProvider GetProviderForOrder(Order order)
    {
        return _providerSelector.SelectOptimalProvider(order.Amount, order.PaymentMode);
    }

    private static OrderResponse MapToResponse(Order order) => new()
    {
        OrderId = order.OrderId,
        Amount = order.Amount,
        Status = order.Status.ToString(),
        PaymentMode = order.PaymentMode.ToString(),
        ProviderName = order.ProviderName,
        ProviderOrderId = order.ProviderOrderId,
        Fees = order.Fees.Select(f => new FeeDto { Name = f.Name, Amount = f.Amount }).ToList(),
        Products = order.Products.Select(p => new ProductDto { Name = p.Name, UnitPrice = p.UnitPrice }).ToList(),
        CreatedDate = order.CreatedDate
    };
}
