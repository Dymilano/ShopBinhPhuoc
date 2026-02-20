using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        ViewData["Title"] = "Tổng quan";
        ViewBag.ProductCount = _db.Products.Count(p => p.IsActive);
        ViewBag.CategoryCount = _db.Categories.Count(c => c.IsActive);
        ViewBag.PostCount = _db.Posts.Count(p => p.IsActive);
        ViewBag.OrderCount = _db.Orders.Count();
        ViewBag.PendingOrders = _db.Orders.Count(o => o.Status == "pending");
        ViewBag.CompletedOrders = _db.Orders.Count(o => o.Status == "completed");
        ViewBag.ContactCount = _db.ContactMessages.Count(m => !m.IsRead);
        ViewBag.UserCount = _db.Users.Count();
        ViewBag.RecentOrders = _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .ToList();
        ViewBag.RecentProducts = _db.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();
        ViewBag.RecentPosts = _db.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();
        return View();
    }
}
