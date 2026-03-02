using System.Text.RegularExpressions;
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
        if (!items.Any())
        {
            TempData["Error"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm vào giỏ hàng trước khi thanh toán.";
            return RedirectToAction("Index", "Cart");
        }
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
        
        // Validate phone format
        phone = phone.Trim();
        if (!Regex.IsMatch(phone, @"^[0-9]{10,11}$"))
        {
            ViewData["Title"] = "Thanh toán";
            ViewBag.CartItems = items;
            ViewBag.CartTotal = _cart.GetTotal();
            ViewBag.Error = "Số điện thoại phải có 10-11 chữ số.";
            ViewBag.CustomerName = customerName;
            ViewBag.CustomerEmail = email;
            ViewBag.CustomerPhone = phone;
            ViewBag.CustomerAddress = address;
            return View();
        }
        
        // Validate email format (nếu có)
        if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ViewData["Title"] = "Thanh toán";
            ViewBag.CartItems = items;
            ViewBag.CartTotal = _cart.GetTotal();
            ViewBag.Error = "Email không hợp lệ. Vui lòng nhập đúng định dạng email.";
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
        
        // Đảm bảo OrderCode được lưu trong cả session và TempData
        HttpContext.Session.SetString("LastOrderCode", orderCode);
        TempData["OrderCode"] = orderCode;
        TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng của bạn: " + orderCode;
        return RedirectToAction(nameof(Success));
    }

    public IActionResult Success()
    {
        ViewData["Title"] = "Đặt hàng thành công";
        // Đọc OrderCode từ TempData hoặc Session (fallback)
        var orderCode = TempData["OrderCode"] as string ?? HttpContext.Session.GetString("LastOrderCode");
        ViewBag.OrderCode = orderCode;
        return View();
    }
}
