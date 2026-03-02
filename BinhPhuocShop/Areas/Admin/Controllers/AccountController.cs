using System.Security.Cryptography;
using System.Text;
using BinhPhuocShop.Data;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly ActivityLogService _activityLog;

    public AccountController(AppDbContext db, ActivityLogService activityLog)
    {
        _db = db;
        _activityLog = activityLog;
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
        await _activityLog.LogAsync("Login", "User", user.Id, user.Email, $"Admin đăng nhập - {user.Role}");
        TempData["Success"] = "Đăng nhập thành công!";
        return Redirect(returnUrl ?? Url.Action("Index", "Dashboard") ?? "/Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var email = HttpContext.Session.GetString("UserEmail");
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var id))
            await _activityLog.LogAsync("Logout", "User", id, email, "Admin đăng xuất");
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
