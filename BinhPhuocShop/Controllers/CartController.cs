using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class CartController : StoreControllerBase
{
    private readonly CartService _cart;
    private readonly AppDbContext _db;

    public CartController(AppDbContext db, CartService cart) : base(db, cart)
    {
        _db = db;
        _cart = cart;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Giỏ hàng";
        ViewBag.CartItems = _cart.GetCart();
        ViewBag.CartTotal = _cart.GetTotal();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, string? size = null, int quantity = 1)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null || !product.IsActive) return Json(new { ok = false, msg = "Sản phẩm không tồn tại" });
        var price = product.SalePrice ?? product.Price;
        _cart.Add(productId, product.Name, product.ImageUrl, size, price, quantity);
        return Json(new { ok = true, count = _cart.GetCount(), total = _cart.GetTotal(), msg = "Đã thêm vào giỏ hàng" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, string? size, int quantity)
    {
        _cart.Update(productId, size, quantity);
        return Json(new { ok = true, count = _cart.GetCount(), total = _cart.GetTotal() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId, string? size)
    {
        _cart.Remove(productId, size);
        return Json(new { ok = true, count = _cart.GetCount(), total = _cart.GetTotal() });
    }

    public IActionResult GetCount()
    {
        return Json(new { count = _cart.GetCount(), total = _cart.GetTotal() });
    }
}
