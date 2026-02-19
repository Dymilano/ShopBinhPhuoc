using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class BlogController : StoreControllerBase
{
    public BlogController(AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

    public async Task<IActionResult> Index(string? category, int page = 1, int pageSize = 9)
    {
        var query = Db.Posts.Where(p => p.IsActive);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.BlogCategory == category);
        query = query.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt);
        string title = "Bài viết";
        if (!string.IsNullOrEmpty(category))
        {
            title = category switch
            {
                "phong-cach" => "Phong cách",
                "tin-tuc-moi-ve-mulgati" => "Tin tức - Khuyến mãi",
                "bao-chi" => "Báo chí",
                "goc-review" => "Góc Review",
                _ => "Bài viết"
            };
        }
        ViewData["Title"] = title;
        var total = await query.CountAsync();
        var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Posts = list;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Category = category;
        ViewBag.BlogCategories = new[] {
            ("", "Tất cả"),
            ("phong-cach", "Phong cách"),
            ("tin-tuc-moi-ve-mulgati", "Tin tức - Khuyến mãi"),
            ("bao-chi", "Báo chí"),
            ("goc-review", "Góc Review"),
        };
        return View();
    }

    public async Task<IActionResult> Detail(int id)
    {
        var post = await Db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (post == null) return NotFound();
        post.ViewCount++;
        await Db.SaveChangesAsync();
        ViewData["Title"] = post.Title;
        ViewBag.LatestPosts = await Db.Posts.Where(p => p.IsActive && p.Id != id).OrderByDescending(p => p.PublishedAt ?? p.CreatedAt).Take(5).ToListAsync();
        return View(post);
    }
}
