using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Thông tin cá nhân";
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction("Login", "Account");
        
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string name, string? phone, string? address)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction("Login", "Account");
        
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        user.Name = name;
        user.Phone = phone;
        user.Address = address;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        HttpContext.Session.SetString("UserName", user.Name);
        TempData["Success"] = "Đã cập nhật thông tin cá nhân thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction("Login", "Account");
        
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        var currentHash = HashPassword(currentPassword);
        if (user.PasswordHash != currentHash)
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction(nameof(Index));
        }
        
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu mới và xác nhận không khớp.";
            return RedirectToAction(nameof(Index));
        }
        
        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        TempData["Success"] = "Đã đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Index));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
