namespace BinhPhuocShop.Models;

public class HomeCategoryBlockViewModel
{
    public Category Category { get; set; } = null!;
    public List<Product> Products { get; set; } = new();
}
