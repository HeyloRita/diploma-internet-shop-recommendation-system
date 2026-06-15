using InternetShop.API.Data;
using InternetShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Services;

public class RestockService(AppDbContext db)
{
    public async Task CheckAndCreateTasksAsync()
    {
        var settings  = await db.AppSettings.FirstAsync();
        int threshold = settings.LowStockThreshold;

        var lowStock = await db.Products
            .Where(p => p.IsActive && p.Stock <= threshold)
            .ToListAsync();

        var lowStockIds = lowStock.Select(p => p.Id).ToHashSet();

        var staleTasks = await db.RestockTasks
            .Where(r => !lowStockIds.Contains(r.ProductId))
            .ToListAsync();
        db.RestockTasks.RemoveRange(staleTasks);

        foreach (var product in lowStock)
        {
            var existing = await db.RestockTasks
                .FirstOrDefaultAsync(r => r.ProductId == product.Id);

            if (existing != null)
            {
                existing.CurrentStock = product.Stock;
                existing.Threshold    = threshold;
            }
            else
            {
                db.RestockTasks.Add(new RestockTask
                {
                    ProductId    = product.Id,
                    CurrentStock = product.Stock,
                    Threshold    = threshold,
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
