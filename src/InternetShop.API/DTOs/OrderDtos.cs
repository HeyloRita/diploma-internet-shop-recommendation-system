namespace InternetShop.API.DTOs;

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(int ProductId, int Quantity);

public record OrderDto(
    int Id, DateTime CreatedAt, decimal TotalAmount,
    string Status, List<OrderItemDto> Items);

public record OrderItemDto(
    int ProductId, string ProductName,
    int Quantity, decimal PriceAtOrder);
