using InternetShop.API.Data;
using InternetShop.API.DTOs;
using InternetShop.API.ML;
using InternetShop.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, RecommendationService recSvc, RestockService restockSvc) : ControllerBase
{
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? level = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = db.SystemLogs.AsQueryable();
        if (!string.IsNullOrEmpty(level))
            query = query.Where(l => l.Level == level);

        int total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new SystemLogDto(l.Id, l.Level, l.Source, l.Message, l.CreatedAt))
            .ToListAsync();

        return Ok(new { total, items });
    }

    [HttpGet("restock")]
    public async Task<IActionResult> GetRestockTasks()
    {
        var tasks = await db.RestockTasks
            .Include(r => r.Product)
            .OrderByDescending(r => r.DetectedAt)
            .Select(r => new RestockTaskDto(
                r.Id, r.ProductId, r.Product.Name,
                r.CurrentStock, r.Threshold, r.DetectedAt))
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var s = await db.AppSettings.FirstAsync();
        return Ok(new AppSettingsDto(s.LowStockThreshold, s.RecommendationCount));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest req)
    {
        var s = await db.AppSettings.FirstAsync();
        s.LowStockThreshold   = req.LowStockThreshold;
        s.RecommendationCount = req.RecommendationCount;
        await db.SaveChangesAsync();

        await restockSvc.CheckAndCreateTasksAsync();

        return Ok(new AppSettingsDto(s.LowStockThreshold, s.RecommendationCount));
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var orders = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status != Models.OrderStatus.Cancelled)
            .ToListAsync();

        var dailySales = orders
            .GroupBy(o => o.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
            .ToList();

        var topProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.Product.Name })
            .OrderByDescending(g => g.Sum(i => i.Quantity))
            .Take(5)
            .Select(g => new TopProductDto(g.Key.ProductId, g.Key.Name, g.Sum(i => i.Quantity)))
            .ToList();

        return Ok(new AnalyticsDto(
            orders.Sum(o => o.TotalAmount),
            orders.Count,
            await db.Users.CountAsync(u => u.Role == Models.UserRole.Customer),
            dailySales,
            topProducts));
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        var items = await db.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price,
                                        p.Stock, p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest req)
    {
        var product = new Models.Product
        {
            Name        = req.Name,
            Description = req.Description,
            Price       = req.Price,
            Stock       = req.Stock,
            ImageUrl    = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl,
            CategoryId  = req.CategoryId,
            IsActive    = true,
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var p = await db.Products.Include(p => p.Category).FirstAsync(p => p.Id == product.Id);
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price,
                                 p.Stock, p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive));
    }

    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest req)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.Name        = req.Name;
        product.Description = req.Description;
        product.Price       = req.Price;
        product.Stock       = req.Stock;
        product.ImageUrl    = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl;
        product.CategoryId  = req.CategoryId;
        product.IsActive    = req.IsActive;
        await db.SaveChangesAsync();

        await restockSvc.CheckAndCreateTasksAsync();

        var p = await db.Products.Include(p => p.Category).FirstAsync(p => p.Id == id);
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price,
                                 p.Stock, p.ImageUrl, p.CategoryId, p.Category.Name, p.IsActive));
    }

    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.IsActive = false;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("recommendations/info")]
    public async Task<IActionResult> GetModelInfo()
    {
        var recs = await db.Recommendations.ToListAsync();

        DateTime? lastTrained   = recs.Count > 0
            ? recs.Max(r => r.CalculatedAt)
            : null;
        int totalRecs           = recs.Count;
        int usersWithRecs       = recs.Select(r => r.UserId).Distinct().Count();
        int trainingEvents      = await db.UserEvents.CountAsync();

        var topRecommended = recs
            .GroupBy(r => r.ProductId)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Join(db.Products.AsEnumerable(),
                  g => g.Key,
                  p => p.Id,
                  (g, p) => new TopRecommendedDto(p.Id, p.Name, g.Count()))
            .ToList();

        return Ok(new ModelInfoDto(
            lastTrained,
            totalRecs,
            usersWithRecs,
            trainingEvents,
            ModelIterations: 20,
            ModelRank: 10,
            Algorithm: "Matrix Factorization",
            topRecommended));
    }

    [HttpPost("recommendations/retrain")]
    public async Task<IActionResult> Retrain()
    {
        await recSvc.TrainAndSaveAsync();
        return Ok(new { message = "Модель переобучена." });
    }
}
