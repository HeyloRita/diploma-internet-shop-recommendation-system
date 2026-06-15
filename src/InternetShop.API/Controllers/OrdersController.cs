using System.Security.Claims;
using InternetShop.API.Data;
using InternetShop.API.DTOs;
using InternetShop.API.Models;
using InternetShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(AppDbContext db, LogService log, RestockService restock) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var orders = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);

        return order is null ? NotFound() : Ok(ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest req)
    {
        if (req.Items.Count == 0)
            return BadRequest(new { message = "Заказ не может быть пустым." });

        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products   = await db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var order = new Order { UserId = UserId };

        foreach (var item in req.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product is null)
                return BadRequest(new { message = $"Товар {item.ProductId} не найден." });
            if (product.Stock < item.Quantity)
                return BadRequest(new { message = $"Недостаточно товара «{product.Name}» на складе." });

            product.Stock -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId    = product.Id,
                Quantity     = item.Quantity,
                PriceAtOrder = product.Price,
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.PriceAtOrder);

        db.UserEvents.AddRange(req.Items.Select(i => new UserEvent
        {
            UserId    = UserId,
            ProductId = i.ProductId,
            Type      = EventType.Purchase,
        }));

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await restock.CheckAndCreateTasksAsync();
        await log.InfoAsync("Orders", $"Новый заказ #{order.Id} от пользователя {UserId}");

        var created = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToDto(created));
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.CreatedAt, o.TotalAmount, o.Status.ToString(),
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.Product.Name, i.Quantity, i.PriceAtOrder)).ToList());
}
