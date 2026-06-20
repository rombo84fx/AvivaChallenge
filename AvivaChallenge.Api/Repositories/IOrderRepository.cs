using AvivaChallenge.Api.Domain;

namespace AvivaChallenge.Api.Repositories;

public interface IOrderRepository
{
    Order Add(Order order);
    Order? GetById(int orderId);
    List<Order> GetAll();
    void Update(Order order);
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private int _nextId = 1;
    private readonly object _lock = new();

    public Order Add(Order order)
    {
        lock (_lock)
        {
            order.OrderId = _nextId++;
            _orders.Add(order);
            return order;
        }
    }

    public Order? GetById(int orderId)
    {
        lock (_lock)
        {
            return _orders.FirstOrDefault(o => o.OrderId == orderId);
        }
    }

    public List<Order> GetAll()
    {
        lock (_lock)
        {
            return _orders.ToList();
        }
    }

    public void Update(Order order)
    {
    }
}
