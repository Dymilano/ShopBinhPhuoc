using System.Text.Json;
using BinhPhuocShop.Models;

namespace BinhPhuocShop.Services;

public class CartService
{
    private const string CartKey = "Cart";
    private readonly IHttpContextAccessor _httpContext;

    public CartService(IHttpContextAccessor httpContext) => _httpContext = httpContext;

    private List<CartItem> GetItems()
    {
        var session = _httpContext.HttpContext?.Session;
        if (session == null) return new List<CartItem>();
        var json = session.GetString(CartKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        try { return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>(); }
        catch { return new List<CartItem>(); }
    }

    private void SaveItems(List<CartItem> items)
    {
        var session = _httpContext.HttpContext?.Session;
        if (session != null)
            session.SetString(CartKey, JsonSerializer.Serialize(items));
    }

    public void Add(int productId, string productName, string? imageUrl, string? size, decimal price, int quantity = 1)
    {
        if (price <= 0) throw new ArgumentException("Giá sản phẩm phải lớn hơn 0.");
        if (quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
        
        var items = GetItems();
        var key = size != null ? $"{productId}:{size}" : productId.ToString();
        var existing = items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (existing != null)
            existing.Quantity += quantity;
        else
            items.Add(new CartItem { ProductId = productId, ProductName = productName, ImageUrl = imageUrl, Size = size, Price = price, Quantity = quantity });
        SaveItems(items);
    }

    public void Update(int productId, string? size, int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Số lượng không được âm.");
        
        var items = GetItems();
        var item = items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (item == null) return;
        if (quantity == 0) { items.Remove(item); SaveItems(items); return; }
        item.Quantity = quantity;
        SaveItems(items);
    }

    public void Remove(int productId, string? size)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId && i.Size == size);
        SaveItems(items);
    }

    public void Clear() => SaveItems(new List<CartItem>());

    public List<CartItem> GetCart() => GetItems();

    public int GetCount() => GetItems().Sum(i => i.Quantity);

    public decimal GetTotal() => GetItems().Sum(i => i.Amount);
}
