namespace BinhPhuocShop.Models;

/// <summary>
/// Dùng cho session cart (JSON serialize)
/// </summary>
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Price * Quantity;
}
