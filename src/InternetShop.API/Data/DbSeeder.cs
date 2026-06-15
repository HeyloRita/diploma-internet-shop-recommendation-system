using InternetShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return; 

        var admin = new User
        {
            Name = "Администратор",
            Email = "admin@shop.ru",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin
        };

        var users = new[]
        {
            new User { Name = "Анна",  Email = "anna@mail.ru",  PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"), Role = UserRole.Customer },
            new User { Name = "Иван",  Email = "ivan@mail.ru",  PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"), Role = UserRole.Customer },
            new User { Name = "Мария", Email = "maria@mail.ru", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"), Role = UserRole.Customer },
        };

        db.Users.Add(admin);
        db.Users.AddRange(users);

        var cats = new[]
        {
            new Category { Name = "Электроника" },
            new Category { Name = "Одежда"      },
            new Category { Name = "Книги"        },
            new Category { Name = "Спорт"        },
        };
        db.Categories.AddRange(cats);
        await db.SaveChangesAsync();

        var products = new[]
        {
            new Product { Name = "Смартфон X12",    Description = "Смартфон с отличной камерой",  Price = 29999, Stock = 50, CategoryId = cats[0].Id },
            new Product { Name = "Наушники Pro",     Description = "Беспроводные наушники",       Price = 4999,  Stock = 30, CategoryId = cats[0].Id },
            new Product { Name = "Ноутбук UltraBook",Description = "Тонкий и лёгкий ноутбук",    Price = 89999, Stock = 15, CategoryId = cats[0].Id },
            new Product { Name = "Планшет Tab S",    Description = "Планшет для работы",         Price = 19999, Stock = 3,  CategoryId = cats[0].Id, LowStockThreshold = 5 },
            new Product { Name = "Куртка зимняя",    Description = "Тёплая куртка",              Price = 7500,  Stock = 20, CategoryId = cats[1].Id },
            new Product { Name = "Кроссовки Run",    Description = "Беговые кроссовки",          Price = 5500,  Stock = 25, CategoryId = cats[1].Id },
            new Product { Name = "Джинсы Slim",      Description = "Классические джинсы",        Price = 3200,  Stock = 2,  CategoryId = cats[1].Id, LowStockThreshold = 5 },
            new Product { Name = "C# для начинающих",Description = "Книга по программированию",  Price = 1200,  Stock = 100,CategoryId = cats[2].Id },
            new Product { Name = "Чистый код",       Description = "Книга Роберта Мартина",      Price = 1500,  Stock = 60, CategoryId = cats[2].Id },
            new Product { Name = "Гантели 5кг",      Description = "Разборные гантели",          Price = 2800,  Stock = 4,  CategoryId = cats[3].Id, LowStockThreshold = 5 },
            new Product { Name = "Скакалка",         Description = "Скакалка с подшипниками",    Price = 600,   Stock = 80, CategoryId = cats[3].Id },
            new Product { Name = "Коврик для йоги",  Description = "Нескользящий коврик",        Price = 1800,  Stock = 35, CategoryId = cats[3].Id },
        };
        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        var order1 = new Order
        {
            UserId = users[0].Id,
            Status = OrderStatus.Delivered,
            TotalAmount = products[0].Price + products[1].Price,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Items = new List<OrderItem>
            {
                new() { ProductId = products[0].Id, Quantity = 1, PriceAtOrder = products[0].Price },
                new() { ProductId = products[1].Id, Quantity = 1, PriceAtOrder = products[1].Price },
            }
        };
        var order2 = new Order
        {
            UserId = users[1].Id,
            Status = OrderStatus.Delivered,
            TotalAmount = products[7].Price * 2,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Items = new List<OrderItem>
            {
                new() { ProductId = products[7].Id, Quantity = 2, PriceAtOrder = products[7].Price },
            }
        };
        db.Orders.AddRange(order1, order2);

        var events = new List<UserEvent>
        {
            new() { UserId = users[0].Id, ProductId = products[0].Id, Type = EventType.Purchase },
            new() { UserId = users[0].Id, ProductId = products[1].Id, Type = EventType.Purchase },
            new() { UserId = users[0].Id, ProductId = products[2].Id, Type = EventType.View     },
            new() { UserId = users[1].Id, ProductId = products[7].Id, Type = EventType.Purchase },
            new() { UserId = users[1].Id, ProductId = products[8].Id, Type = EventType.View     },
            new() { UserId = users[2].Id, ProductId = products[4].Id, Type = EventType.View     },
            new() { UserId = users[2].Id, ProductId = products[5].Id, Type = EventType.Purchase },
        };
        db.UserEvents.AddRange(events);

        await db.SaveChangesAsync();
    }
}
