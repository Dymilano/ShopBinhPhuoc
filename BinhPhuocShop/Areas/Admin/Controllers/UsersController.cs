using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? role, string? search, int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Quản lý người dùng";
        var query = _db.Users.AsQueryable();
        
        if (!string.IsNullOrEmpty(role) && role != "all")
            query = query.Where(u => u.Role == role);
        
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
        
        var total = await query.CountAsync();
        var users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.Users = users;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Role = role;
        ViewBag.Search = search;
        return View();
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm người dùng";
        return View(new User { Role = "Customer" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string password)
    {
        if (await _db.Users.AnyAsync(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
            return View(user);
        }
        
        user.PasswordHash = HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm người dùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa người dùng";
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user, string? newPassword)
    {
        var existing = await _db.Users.FindAsync(id);
        if (existing == null) return NotFound();
        
        if (await _db.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
            return View(user);
        }
        
        existing.Name = user.Name;
        existing.Email = user.Email;
        existing.Phone = user.Phone;
        existing.Address = user.Address;
        existing.Role = user.Role;
        existing.IsActive = user.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(newPassword))
            existing.PasswordHash = HashPassword(newPassword);
        
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật người dùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa người dùng thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
