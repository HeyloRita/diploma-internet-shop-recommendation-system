namespace InternetShop.API.Models;

public class Recommendation
{
    public int Id { get; set; }
    public float Score { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
