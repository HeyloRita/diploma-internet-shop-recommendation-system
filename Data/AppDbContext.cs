using InternetShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<RestockTask> RestockTasks => Set<RestockTask>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("numeric(18,2)");

        mb.Entity<OrderItem>()
            .Property(p => p.PriceAtOrder)
            .HasColumnType("numeric(18,2)");

        mb.Entity<Order>()
            .Property(p => p.TotalAmount)
            .HasColumnType("numeric(18,2)");

        mb.Entity<AppSettings>().HasData(new AppSettings { Id = 1 });
    }
}
