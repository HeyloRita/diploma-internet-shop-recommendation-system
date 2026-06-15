using InternetShop.API.Data;
using InternetShop.API.Models;

namespace InternetShop.API.Services;

public class LogService(AppDbContext db)
{
    public async Task InfoAsync(string source, string message)
        => await WriteAsync("Info", source, message);

    public async Task WarnAsync(string source, string message)
        => await WriteAsync("Warning", source, message);

    public async Task ErrorAsync(string source, string message)
        => await WriteAsync("Error", source, message);

    private async Task WriteAsync(string level, string source, string message)
    {
        db.SystemLogs.Add(new SystemLog { Level = level, Source = source, Message = message });
        await db.SaveChangesAsync();
    }
}
