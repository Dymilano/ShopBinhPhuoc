namespace BinhPhuocShop.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public string Action { get; set; } = string.Empty; // Login, Logout, Register, Create, Update, Delete
    public string EntityType { get; set; } = string.Empty; // User, Product, Order, etc.
    public int? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
