using AvivaChallenge.Api.Domain;
using AvivaChallenge.Api.Models;

namespace AvivaChallenge.Api.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderResponse>> GetOrdersAsync();
    Task<OrderResponse?> GetOrderAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId);
    Task<bool> PayOrderAsync(int orderId);
}
