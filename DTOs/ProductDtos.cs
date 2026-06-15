namespace InternetShop.API.DTOs;

public record ProductDto(
    int Id, string Name, string Description,
    decimal Price, int Stock, string? ImageUrl,
    int CategoryId, string CategoryName, bool IsActive);

public record ProductListResponse(IEnumerable<ProductDto> Items, int TotalCount);

public record ProductFilterRequest(
    string? Search = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 12);
