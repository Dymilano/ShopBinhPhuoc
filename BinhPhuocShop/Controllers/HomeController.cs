using System.Diagnostics;
using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class HomeController : StoreControllerBase
{
    public HomeController(AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Trang chủ";
        var categories = await Db.Categories.Where(c => c.IsActive && c.ParentId == null).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync();
        var categoryIds = categories.Select(c => c.Id).ToList();
        var productCountByCategory = new Dictionary<int, int>();
        foreach (var c in categories)
        {
            var childIds = await Db.Categories.Where(c2 => c2.ParentId == c.Id).Select(c2 => c2.Id).ToListAsync();
            var ids = new List<int> { c.Id }.Union(childIds).ToList();
            var count = await Db.Products.CountAsync(p => ids.Contains(p.CategoryId));
            productCountByCategory[c.Id] = count;
        }
        ViewBag.Categories = categories;
        ViewBag.CategoryProductCount = productCountByCategory;
        ViewBag.ProductCountAll = await Db.Products.CountAsync(p => p.IsActive);
        ViewBag.FeaturedProducts = await Db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();
        ViewBag.BestSellingProducts = await Db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(12)
            .ToListAsync();
        ViewBag.LatestProducts = await Db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();
        var homeBlocks = new List<HomeCategoryBlockViewModel>();
        foreach (var c in categories.Take(2))
        {
            var products = await Db.Products.Include(p => p.Category).Where(p => p.IsActive && (p.CategoryId == c.Id || (p.Category != null && p.Category.ParentId == c.Id))).OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync();
            homeBlocks.Add(new HomeCategoryBlockViewModel { Category = c, Products = products });
        }
        ViewBag.HomeCategoryBlocks = homeBlocks;
        return View();
    }

    public IActionResult About()
    {
        return RedirectToAction("GioiThieu", "Pages");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
