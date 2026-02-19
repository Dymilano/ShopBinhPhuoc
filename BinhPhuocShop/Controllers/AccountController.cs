using System.Security.Cryptography;
using System.Text;
using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class AccountController : StoreControllerBase
{
    public AccountController(AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

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
    public async Task<IActionResult> Register(string name, string email, string password, string? phone = null)
    {
        if (await Db.Users.AnyAsync(u => u.Email == email))
        {
            ViewBag.Error = "Email đã được sử dụng.";
            return View();
        }
        var user = new User
        {
            Name = name,
            Email = email,
            Phone = phone,
            PasswordHash = HashPassword(password),
            IsActive = true
        };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        TempData["Success"] = "Đăng ký thành công!";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
