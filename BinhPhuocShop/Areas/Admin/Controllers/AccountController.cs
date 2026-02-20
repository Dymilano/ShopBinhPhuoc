using System.Security.Cryptography;
using System.Text;
using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Nếu đã đăng nhập, redirect về dashboard
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var hash = HashPassword(password);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash && u.IsActive);
        if (user == null)
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View();
        }
        // Chỉ cho phép Admin và Manager đăng nhập vào admin panel
        if (user.Role != "Admin" && user.Role != "Manager")
        {
            ViewBag.Error = "Bạn không có quyền truy cập vào admin panel.";
            return View();
        }
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("IsAdmin", user.Role == "Admin" ? "true" : "false");
        return Redirect(returnUrl ?? Url.Action("Index", "Dashboard") ?? "/admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
