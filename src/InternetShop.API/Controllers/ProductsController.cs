using InternetShop.API.Data;
using InternetShop.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> GetAll([FromQuery] ProductFilterRequest filter)
    {
        var query = db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.ToLower().Contains(filter.Search.ToLower()));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        int total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock,
                                        p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive))
            .ToListAsync();

        return Ok(new ProductListResponse(items, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var p = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return NotFound();
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock,
                                 p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await db.Categories.Select(c => new { c.Id, c.Name }).ToListAsync());
}
