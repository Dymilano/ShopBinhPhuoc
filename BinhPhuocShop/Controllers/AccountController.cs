using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class AccountController : StoreControllerBase
{
    private readonly ActivityLogService _activityLog;

    public AccountController(AppDbContext db, CartService cart, ActivityLogService activityLog) : base(db, cart)
    {
        _activityLog = activityLog;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Đăng nhập";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        var hash = HashPassword(password);
        var user = await Db.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash && u.IsActive);
        if (user == null)
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View();
        }
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
        await _activityLog.LogAsync("Login", "User", user.Id, user.Email, "Đăng nhập thành công");
        TempData["Success"] = "Đăng nhập thành công! Chào mừng bạn quay trở lại.";
        return Redirect(returnUrl ?? "/");
    }

    [HttpGet]
    public IActionResult Register()
    {
        ViewData["Title"] = "Đăng ký";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string? name, string? email, string? password, string? confirmPassword, string? phone = null, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng điền đầy đủ Họ tên, Email và Mật khẩu.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        
        if (string.IsNullOrWhiteSpace(phone))
        {
            ViewBag.Error = "Vui lòng nhập số điện thoại.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        
        if (string.IsNullOrWhiteSpace(address))
        {
            ViewBag.Error = "Vui lòng nhập địa chỉ.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        if (password.Length < 6)
        {
            ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        if (password != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu và xác nhận mật khẩu không khớp.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        email = email.Trim().ToLowerInvariant();
        
        // Validate email format
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ViewBag.Error = "Email không hợp lệ. Vui lòng nhập đúng định dạng email.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        
        // Validate phone format
        phone = phone?.Trim();
        if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^[0-9]{10,11}$"))
        {
            ViewBag.Error = "Số điện thoại phải có 10-11 chữ số.";
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
        
        try
        {
            if (await Db.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == email))
            {
                ViewBag.Error = "Email đã được sử dụng.";
                ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
                return View();
            }
            var user = new User
            {
                Name = name.Trim(),
                Email = email,
                Phone = phone.Trim(),
                Address = address?.Trim(),
                PasswordHash = HashPassword(password),
                Role = "Customer",
                IsActive = true
            };
            Db.Users.Add(user);
            await Db.SaveChangesAsync();
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            try
            {
                await _activityLog.LogAsync("Register", "User", user.Id, user.Email, "Đăng ký tài khoản mới");
            }
            catch { /* Không chặn đăng ký nếu ghi log lỗi */ }
            TempData["Success"] = "Đăng ký thành công!";
            return Redirect("/");
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Lỗi đăng ký: " + (ex.InnerException?.Message ?? ex.Message);
            ViewBag.Name = name; ViewBag.Email = email; ViewBag.Phone = phone; ViewBag.Address = address;
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var email = HttpContext.Session.GetString("UserEmail");
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var id))
            await _activityLog.LogAsync("Logout", "User", id, email, "Đăng xuất");
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction(nameof(Login));
        var user = await Db.Users.FindAsync(id);
        if (user == null) return NotFound();
        ViewData["Title"] = "Thông tin cá nhân";
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string name, string? phone, string? address)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction(nameof(Login));
        
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Họ tên không được để trống.";
            return RedirectToAction(nameof(Profile));
        }
        
        if (string.IsNullOrWhiteSpace(phone))
        {
            TempData["Error"] = "Số điện thoại không được để trống.";
            return RedirectToAction(nameof(Profile));
        }
        
        if (string.IsNullOrWhiteSpace(address))
        {
            TempData["Error"] = "Địa chỉ không được để trống.";
            return RedirectToAction(nameof(Profile));
        }
        
        // Validate phone format
        phone = phone.Trim();
        if (!Regex.IsMatch(phone, @"^[0-9]{10,11}$"))
        {
            TempData["Error"] = "Số điện thoại phải có 10-11 chữ số.";
            return RedirectToAction(nameof(Profile));
        }
        
        var user = await Db.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        user.Name = name.Trim();
        user.Phone = phone;
        user.Address = address.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        HttpContext.Session.SetString("UserName", user.Name);
        try
        {
            await _activityLog.LogAsync("Update", "User", user.Id, user.Email, "Cập nhật thông tin cá nhân");
        }
        catch { }
        TempData["Success"] = "Đã cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return RedirectToAction(nameof(Login));
        var user = await Db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.PasswordHash != HashPassword(currentPassword))
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction(nameof(Profile));
        }
        if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu mới và xác nhận không khớp.";
            return RedirectToAction(nameof(Profile));
        }
        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("ChangePassword", "User", user.Id, null, "Đổi mật khẩu");
        TempData["Success"] = "Đã đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
