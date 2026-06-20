using AvivaChallenge.Api.Models;
using AvivaChallenge.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvivaChallenge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var result = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetOrders()
    {
        var result = await _orderService.GetOrdersAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    {
        var result = await _orderService.GetOrderAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var success = await _orderService.CancelOrderAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPut("{id:int}/pay")]
    public async Task<IActionResult> PayOrder(int id)
    {
        var success = await _orderService.PayOrderAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
