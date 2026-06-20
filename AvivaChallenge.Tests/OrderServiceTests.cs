using AvivaChallenge.Api.Domain;
using AvivaChallenge.Api.Models;
using AvivaChallenge.Api.Providers;
using AvivaChallenge.Api.Repositories;
using AvivaChallenge.Api.Services;
using NSubstitute;
using Xunit;

namespace AvivaChallenge.Tests;

public class OrderServiceTests
{
    private readonly IPaymentProviderSelector _selector;
    private readonly IPaymentProvider _provider;
    private readonly IOrderRepository _repository;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _selector = Substitute.For<IPaymentProviderSelector>();
        _provider = Substitute.For<IPaymentProvider>();
        _repository = new InMemoryOrderRepository();
        _service = new OrderService(_selector, _repository);

        _provider.Name.Returns("TestProvider");
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsOrderResponse()
    {
        var request = new CreateOrderRequest
        {
            PaymentMode = "CreditCard",
            Products = [new ProductDto { Name = "Laptop", UnitPrice = 1000m }]
        };

        _selector.SelectOptimalProvider(1000m, PaymentMode.CreditCard).Returns(_provider);
        _provider.CreateOrderAsync(PaymentMode.CreditCard, Arg.Any<List<Product>>())
            .Returns(new ProviderOrder
            {
                ProviderOrderId = "prov-123",
                Amount = 1000m,
                Status = "Pending",
                Fees = [new Fee { Name = "Commission", Amount = 10m }],
                Products = [new Product { Name = "Laptop", UnitPrice = 1000m }],
                CreatedDate = DateTime.UtcNow
            });

        var result = await _service.CreateOrderAsync(request);

        Assert.Equal(1, result.OrderId);
        Assert.Equal(1000m, result.Amount);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("TestProvider", result.ProviderName);
        Assert.Equal("prov-123", result.ProviderOrderId);
        Assert.Single(result.Products);
        Assert.Single(result.Fees);
    }

    [Fact]
    public async Task CreateOrder_InvalidPaymentMode_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            PaymentMode = "Bitcoin",
            Products = [new ProductDto { Name = "Laptop", UnitPrice = 1000m }]
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrder_NoProducts_ThrowsArgumentException()
    {
        var request = new CreateOrderRequest
        {
            PaymentMode = "Cash",
            Products = []
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task GetOrders_ReturnsAllOrders()
    {
        await CreateSampleOrder();
        await CreateSampleOrder();

        var result = await _service.GetOrdersAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetOrder_ExistingId_ReturnsOrder()
    {
        await CreateSampleOrder();

        var result = await _service.GetOrderAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.OrderId);
    }

    [Fact]
    public async Task GetOrder_NonExistingId_ReturnsNull()
    {
        var result = await _service.GetOrderAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelOrder_ExistingOrder_ReturnsTrueAndUpdateStatus()
    {
        await CreateSampleOrder();

        _selector.SelectOptimalProvider(Arg.Any<decimal>(), Arg.Any<PaymentMode>()).Returns(_provider);
        _provider.CancelOrderAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var result = await _service.CancelOrderAsync(1);

        Assert.True(result);
        var order = await _service.GetOrderAsync(1);
        Assert.Equal("Cancelled", order!.Status);
    }

    [Fact]
    public async Task CancelOrder_NonExistingOrder_ReturnsFalse()
    {
        var result = await _service.CancelOrderAsync(999);
        Assert.False(result);
    }

    [Fact]
    public async Task PayOrder_ExistingOrder_ReturnsTrueAndUpdateStatus()
    {
        await CreateSampleOrder();

        _selector.SelectOptimalProvider(Arg.Any<decimal>(), Arg.Any<PaymentMode>()).Returns(_provider);
        _provider.PayOrderAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var result = await _service.PayOrderAsync(1);

        Assert.True(result);
        var order = await _service.GetOrderAsync(1);
        Assert.Equal("Paid", order!.Status);
    }

    [Fact]
    public async Task PayOrder_NonExistingOrder_ReturnsFalse()
    {
        var result = await _service.PayOrderAsync(999);
        Assert.False(result);
    }

    private async Task CreateSampleOrder()
    {
        _selector.SelectOptimalProvider(Arg.Any<decimal>(), Arg.Any<PaymentMode>()).Returns(_provider);
        _provider.CreateOrderAsync(Arg.Any<PaymentMode>(), Arg.Any<List<Product>>())
            .Returns(new ProviderOrder
            {
                ProviderOrderId = Guid.NewGuid().ToString(),
                Amount = 1000m,
                Status = "Pending",
                Fees = [],
                Products = [new Product { Name = "Test", UnitPrice = 1000m }],
                CreatedDate = DateTime.UtcNow
            });

        await _service.CreateOrderAsync(new CreateOrderRequest
        {
            PaymentMode = "Cash",
            Products = [new ProductDto { Name = "Test", UnitPrice = 1000m }]
        });
    }
}
