namespace InternetShop.API.Models;

public class AppSettings
{
    public int Id { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public int RecommendationCount { get; set; } = 10;
}
