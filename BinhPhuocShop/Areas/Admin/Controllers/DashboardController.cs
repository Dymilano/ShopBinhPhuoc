using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class DashboardController : AdminControllerBase
{
    public DashboardController(AppDbContext db) : base(db) { }

    public IActionResult Index()
    {
        ViewData["Title"] = "Tổng quan";
        ViewBag.ProductCount = Db.Products.Count(p => p.IsActive);
        ViewBag.CategoryCount = Db.Categories.Count(c => c.IsActive);
        ViewBag.PostCount = Db.Posts.Count(p => p.IsActive);
        ViewBag.OrderCount = Db.Orders.Count();
        ViewBag.PendingOrders = Db.Orders.Count(o => o.Status == "pending");
        ViewBag.CompletedOrders = Db.Orders.Count(o => o.Status == "completed");
        ViewBag.ContactCount = Db.ContactMessages.Count(m => !m.IsRead);
        ViewBag.UserCount = Db.Users.Count();
        ViewBag.AdminCount = Db.Users.Count(u => u.Role == "Admin");
        ViewBag.ManagerCount = Db.Users.Count(u => u.Role == "Manager");
        ViewBag.CustomerCount = Db.Users.Count(u => u.Role == "Customer");
        ViewBag.ActiveUserCount = Db.Users.Count(u => u.IsActive);
        ViewBag.TotalRevenue = Db.Orders.Where(o => o.Status == "completed").Sum(o => (decimal?)o.TotalAmount) ?? 0;
        ViewBag.RecentOrders = Db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .ToList();
        ViewBag.RecentProducts = Db.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();
        ViewBag.RecentPosts = Db.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();
        return View();
    }
}
