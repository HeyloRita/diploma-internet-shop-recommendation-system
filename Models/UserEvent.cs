namespace InternetShop.API.Models;

public class UserEvent
{
    public int Id { get; set; }
    public EventType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

public enum EventType { View, Purchase }
