using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        ViewData["Title"] = "Tổng quan";
        ViewBag.ProductCount = _db.Products.Count();
        ViewBag.CategoryCount = _db.Categories.Count();
        ViewBag.PostCount = _db.Posts.Count();
        ViewBag.OrderCount = _db.Orders.Count();
        ViewBag.ContactCount = _db.ContactMessages.Count(m => !m.IsRead);
        ViewBag.UserCount = _db.Users.Count();
        ViewBag.RecentOrders = _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .ToList();
        return View();
    }
}
