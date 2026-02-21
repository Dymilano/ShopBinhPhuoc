using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class CheckoutController : StoreControllerBase
{
    private readonly CartService _cart;
    private readonly AppDbContext _db;

    public CheckoutController(AppDbContext db, CartService cart) : base(db, cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var items = _cart.GetCart();
        if (!items.Any()) return RedirectToAction("Index", "Cart");
        ViewData["Title"] = "Thanh toán";
        ViewBag.CartItems = items;
        ViewBag.CartTotal = _cart.GetTotal();
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var uid))
        {
            var user = await _db.Users.FindAsync(uid);
            if (user != null)
            {
                ViewBag.CustomerName = user.Name;
                ViewBag.CustomerEmail = user.Email;
                ViewBag.CustomerPhone = user.Phone;
                ViewBag.CustomerAddress = user.Address;
            }
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string customerName, string phone, string email, string address, string? note)
    {
        var items = _cart.GetCart();
        if (!items.Any())
        {
            TempData["Error"] = "Giỏ hàng trống.";
            return RedirectToAction("Index", "Cart");
        }
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
        {
            ViewData["Title"] = "Thanh toán";
            ViewBag.CartItems = items;
            ViewBag.CartTotal = _cart.GetTotal();
            ViewBag.Error = "Vui lòng nhập đầy đủ Họ tên, Số điện thoại và Địa chỉ.";
            ViewBag.CustomerName = customerName;
            ViewBag.CustomerEmail = email;
            ViewBag.CustomerPhone = phone;
            ViewBag.CustomerAddress = address;
            return View();
        }
        var userIdStr = HttpContext.Session.GetString("UserId");
        int? userId = !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var uid) ? uid : null;
        var orderCode = "BP" + DateTime.UtcNow.ToString("yyMMddHHmmss");
        var order = new Order
        {
            UserId = userId,
            OrderCode = orderCode,
            CustomerName = customerName.Trim(),
            Phone = phone.Trim(),
            Email = (email ?? "").Trim(),
            Address = address.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Status = "pending",
            TotalAmount = _cart.GetTotal()
        };
        foreach (var item in items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductImageUrl = item.ImageUrl,
                Size = item.Size,
                Price = item.Price,
                Quantity = item.Quantity
            });
        }
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        _cart.Clear();
        TempData["OrderCode"] = orderCode;
        return RedirectToAction(nameof(Success));
    }

    public IActionResult Success()
    {
        ViewData["Title"] = "Đặt hàng thành công";
        ViewBag.OrderCode = TempData["OrderCode"];
        return View();
    }
}
