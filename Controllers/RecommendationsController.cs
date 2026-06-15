using System.Security.Claims;
using InternetShop.API.Data;
using InternetShop.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetMyRecommendations()
    {
        var settings = await db.AppSettings.FirstAsync();

        var recs = await db.Recommendations
            .Include(r => r.Product).ThenInclude(p => p.Category)
            .Where(r => r.UserId == UserId && r.Product.IsActive)
            .OrderByDescending(r => r.Score)
            .Take(settings.RecommendationCount)
            .ToListAsync();

        if (recs.Count == 0)
        {
            var popular = await db.UserEvents
                .Where(e => e.Product.IsActive)
                .GroupBy(e => e.ProductId)
                .OrderByDescending(g => g.Count())
                .Take(settings.RecommendationCount)
                .Select(g => g.First().Product)
                .Include(p => p.Category)
                .ToListAsync();

            return Ok(popular.Select(p =>
                new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock,
                               p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive)));
        }

        return Ok(recs.Select(r =>
            new ProductDto(r.Product.Id, r.Product.Name, r.Product.Description,
                           r.Product.Price, r.Product.Stock, r.Product.ImageUrl,
                           r.Product.CategoryId, r.Product.Category.Name, r.Product.IsActive)));
    }
}
