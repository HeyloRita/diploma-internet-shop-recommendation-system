using System.Security.Claims;
using InternetShop.API.Data;
using InternetShop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("view/{productId:int}")]
    public async Task<IActionResult> RecordView(int productId)
    {
        var product = await db.Products.FindAsync(productId);
        if (product is null) return NotFound();

        db.UserEvents.Add(new UserEvent
        {
            UserId    = UserId,
            ProductId = productId,
            Type      = EventType.View,
        });
        await db.SaveChangesAsync();
        return Ok();
    }
}
