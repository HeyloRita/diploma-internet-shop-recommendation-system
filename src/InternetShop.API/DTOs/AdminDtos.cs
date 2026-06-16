namespace InternetShop.API.DTOs;

public record RestockTaskDto(
    int Id, int ProductId, string ProductName,
    int CurrentStock, int Threshold, DateTime DetectedAt);

public record SystemLogDto(
    int Id, string Level, string Source, string Message, DateTime CreatedAt);

public record AppSettingsDto(int LowStockThreshold, int RecommendationCount);
public record UpdateSettingsRequest(int LowStockThreshold, int RecommendationCount);

public record AnalyticsDto(
    decimal TotalRevenue,
    int TotalOrders,
    int TotalUsers,
    List<DailySalesDto> DailySales,
    List<TopProductDto> TopProducts);

public record DailySalesDto(DateTime Date, decimal Revenue, int OrderCount);
public record TopProductDto(int ProductId, string ProductName, int SoldCount);

public record ModelInfoDto(
    DateTime? LastTrainedAt,
    int TotalRecommendations,
    int UsersWithRecs,
    int TrainingEventsCount,
    int ModelIterations,
    int ModelRank,
    string Algorithm,
    List<TopRecommendedDto> TopRecommended);

public record TopRecommendedDto(int ProductId, string ProductName, int TimesRecommended);

public record CreateProductRequest(
    string Name, string Description,
    decimal Price, int Stock,
    string? ImageUrl, int CategoryId);

public record UpdateProductRequest(
    string Name, string Description,
    decimal Price, int Stock,
    string? ImageUrl, int CategoryId, bool IsActive);
