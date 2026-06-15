namespace InternetShop.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserEvent> Events { get; set; } = new List<UserEvent>();
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}

public enum UserRole { Customer, Admin }
