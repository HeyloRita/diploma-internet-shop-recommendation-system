namespace InternetShop.WPF.Models;

public record AuthResponse(string Token, string Name, string Email, string Role);

public record ProductDto(
    int Id, string Name, string Description,
    decimal Price, int Stock, string? ImageUrl,
    int CategoryId, string CategoryName, bool IsActive);

public record ProductListResponse(IEnumerable<ProductDto> Items, int TotalCount);

public record CategoryDto(int Id, string Name);

public record OrderDto(
    int Id, DateTime CreatedAt, decimal TotalAmount,
    string Status, List<OrderItemDto> Items);

public record OrderItemDto(
    int ProductId, string ProductName,
    int Quantity, decimal PriceAtOrder)
{
    public decimal LineTotal => Quantity * PriceAtOrder;
}

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(int ProductId, int Quantity);

public record RestockTaskDto(
    int Id, int ProductId, string ProductName,
    int CurrentStock, int Threshold, DateTime DetectedAt);

public record SystemLogDto(
    int Id, string Level, string Source, string Message, DateTime CreatedAt);

public record AppSettingsDto(int LowStockThreshold, int RecommendationCount);

public record AnalyticsDto(
    decimal TotalRevenue, int TotalOrders, int TotalUsers,
    List<DailySalesDto> DailySales, List<TopProductDto> TopProducts);

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

public record AdminCreateProductRequest(
    string Name, string Description,
    decimal Price, int Stock,
    string? ImageUrl, int CategoryId);

public record AdminUpdateProductRequest(
    string Name, string Description,
    decimal Price, int Stock,
    string? ImageUrl, int CategoryId, bool IsActive);
