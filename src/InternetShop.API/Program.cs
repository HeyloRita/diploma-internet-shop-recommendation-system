using System.Text;
using InternetShop.API.Data;
using InternetShop.API.ML;
using InternetShop.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

try
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Console.WriteLine("=== InternetShop API запускается ===");

    var builder = WebApplication.CreateBuilder(args);

    Console.WriteLine($"[OK] Строка подключения: {builder.Configuration.GetConnectionString("DefaultConnection")}");

    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    var jwtKey = builder.Configuration["Jwt:Key"]!;
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = builder.Configuration["Jwt:Issuer"],
                ValidAudience            = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddScoped<JwtService>();
    builder.Services.AddScoped<LogService>();
    builder.Services.AddScoped<RestockService>();
    builder.Services.AddScoped<RecommendationService>();
    builder.Services.AddHostedService<RecommendationBackgroundService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "InternetShop API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In          = ParameterLocation.Header,
            Description = "Введите JWT токен",
            Name        = "Authorization",
            Type        = SecuritySchemeType.Http,
            Scheme      = "bearer",
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddCors(opt =>
        opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db      = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restock = scope.ServiceProvider.GetRequiredService<RestockService>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        Console.WriteLine("[...] Подключение к базе данных...");

        const int maxAttempts = 10;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await DbSeeder.SeedAsync(db);
                await restock.CheckAndCreateTasksAsync();
                Console.WriteLine("[OK] База данных готова.");
                break;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[!] БД недоступна (попытка {attempt}/{maxAttempts}): {ex.Message}");
                Console.ResetColor();
                logger.LogWarning("БД недоступна (попытка {Attempt}/{Max}): {Message}. Повтор через 3 сек...",
                    attempt, maxAttempts, ex.Message);
                await Task.Delay(3000);
            }
        }
    }

    app.UseCors();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Console.WriteLine("[OK] API запущен. Слушает http://localhost:5000");
    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    Console.WriteLine("=== КРИТИЧЕСКАЯ ОШИБКА — API не запустился ===");
    Console.WriteLine();
    Console.WriteLine($"Тип:    {ex.GetType().Name}");
    Console.WriteLine($"Причина: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Детали: {ex.InnerException.Message}");
    Console.WriteLine();
    Console.WriteLine("Полный стек вызовов:");
    Console.WriteLine(ex.ToString());
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey();
}
