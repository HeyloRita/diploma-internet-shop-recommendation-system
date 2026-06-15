namespace InternetShop.API.Models;

public class RestockTask
{
    public int Id { get; set; }
    public int CurrentStock { get; set; }
    public int Threshold { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
