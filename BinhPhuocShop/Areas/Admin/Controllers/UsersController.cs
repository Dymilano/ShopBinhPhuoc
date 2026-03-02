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
public class UsersController : AdminControllerBase
{
    public UsersController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index(string? role, string? search, int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Quản lý người dùng";
        var query = Db.Users.AsQueryable();
        
        // Statistics
        ViewBag.TotalUsers = await Db.Users.CountAsync();
        ViewBag.TotalAdmins = await Db.Users.CountAsync(u => u.Role == "Admin");
        ViewBag.TotalManagers = await Db.Users.CountAsync(u => u.Role == "Manager");
        ViewBag.TotalCustomers = await Db.Users.CountAsync(u => u.Role == "Customer");
        ViewBag.ActiveUsers = await Db.Users.CountAsync(u => u.IsActive);
        ViewBag.InactiveUsers = await Db.Users.CountAsync(u => !u.IsActive);
        
        if (!string.IsNullOrEmpty(role) && role != "all")
            query = query.Where(u => u.Role == role);
        
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
        
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.Users = users;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.Role = role ?? "all";
        ViewBag.Search = search;
        return View();
    }

    public IActionResult Create(string? role = null)
    {
        ViewData["Title"] = role == "Admin" ? "Cấp tài khoản Admin" : "Thêm người dùng";
        var user = new User
        {
            Role = role == "Admin" ? "Admin" : role == "Manager" ? "Manager" : "Customer",
            IsActive = true
        };
        ViewBag.IsCreateAdmin = (role == "Admin");
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError("Email", "Email không được để trống.");
            return View(user);
        }
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
            ViewBag.IsCreateAdmin = (user.Role == "Admin");
            return View(user);
        }
        
        if (string.IsNullOrWhiteSpace(user.Phone))
        {
            ModelState.AddModelError("Phone", "Số điện thoại không được để trống.");
            ViewBag.IsCreateAdmin = (user.Role == "Admin");
            return View(user);
        }
        
        if (string.IsNullOrWhiteSpace(user.Address))
        {
            ModelState.AddModelError("Address", "Địa chỉ không được để trống.");
            ViewBag.IsCreateAdmin = (user.Role == "Admin");
            return View(user);
        }
        
        // Validate phone format
        var phone = user.Phone.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9]{10,11}$"))
        {
            ModelState.AddModelError("Phone", "Số điện thoại phải có 10-11 chữ số.");
            ViewBag.IsCreateAdmin = (user.Role == "Admin");
            return View(user);
        }
        
        var email = user.Email.Trim().ToLowerInvariant();
        if (await Db.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
            ViewBag.IsCreateAdmin = (user.Role == "Admin");
            return View(user);
        }
        
        user.Email = email;
        user.PasswordHash = HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.Name = user.Name?.Trim() ?? "";
        user.Phone = phone;
        user.Address = user.Address.Trim();
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        TempData["Success"] = user.Role == "Admin" ? "Đã cấp tài khoản Admin thành công." : "Đã thêm người dùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa người dùng";
        var user = await Db.Users.FindAsync(id);
        if (user == null) return NotFound();
        
        // Get user statistics
        ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
        ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user, string? newPassword)
    {
        var existing = await Db.Users.FindAsync(id);
        if (existing == null) return NotFound();
        
        if (await Db.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng.");
            ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
            ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            return View(user);
        }
        
        if (string.IsNullOrWhiteSpace(user.Phone))
        {
            ModelState.AddModelError("Phone", "Số điện thoại không được để trống.");
            ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
            ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            return View(user);
        }
        
        if (string.IsNullOrWhiteSpace(user.Address))
        {
            ModelState.AddModelError("Address", "Địa chỉ không được để trống.");
            ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
            ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            return View(user);
        }
        
        // Validate phone format
        var phone = user.Phone.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9]{10,11}$"))
        {
            ModelState.AddModelError("Phone", "Số điện thoại phải có 10-11 chữ số.");
            ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
            ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            return View(user);
        }
        
        existing.Name = user.Name?.Trim() ?? "";
        existing.Email = user.Email;
        existing.Phone = phone;
        existing.Address = user.Address?.Trim() ?? "";
        existing.Role = user.Role;
        existing.IsActive = user.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(newPassword))
        {
            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                ViewBag.TotalOrders = await Db.Orders.CountAsync(o => o.UserId == id);
                ViewBag.TotalSpent = await Db.Orders.Where(o => o.UserId == id && o.Status == "completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
                return View(user);
            }
            existing.PasswordHash = HashPassword(newPassword);
        }
        
        await Db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật người dùng thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await Db.Users.FindAsync(id);
        if (user != null)
        {
            // Check if user has orders
            var hasOrders = await Db.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["Error"] = "Không thể xóa người dùng này vì đã có đơn hàng. Vui lòng vô hiệu hóa tài khoản thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }
            
            Db.Users.Remove(user);
            await Db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa người dùng thành công.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var user = await Db.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await Db.SaveChangesAsync();
            TempData["Success"] = $"Đã {(user.IsActive ? "kích hoạt" : "vô hiệu hóa")} tài khoản thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
