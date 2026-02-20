using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class PostsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PostsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Bài viết";
        return View(await _db.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm bài viết";
        return View(new Post { PublishedAt = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Post model, IFormFile? imageFile)
    {
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Title);
        if (imageFile != null) model.ImageUrl = await SaveFile(imageFile, "posts");
        model.CreatedAt = DateTime.UtcNow;
        _db.Posts.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa bài viết";
        var item = await _db.Posts.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Post model, IFormFile? imageFile)
    {
        if (id != model.Id) return NotFound();
        var item = await _db.Posts.FindAsync(id);
        if (item == null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Title);
        if (imageFile != null) item.ImageUrl = await SaveFile(imageFile, "posts");
        item.Title = model.Title;
        item.Slug = model.Slug;
        item.Summary = model.Summary;
        item.Content = model.Content;
        item.IsActive = model.IsActive;
        item.IsPinned = model.IsPinned;
        item.PublishedAt = model.PublishedAt;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Posts.FindAsync(id);
        if (item != null)
        {
            _db.Posts.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> SaveFile(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var name = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        using (var stream = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{name}";
    }

    private static string Slugify(string text) => string.IsNullOrWhiteSpace(text) ? "" : string.Join("-", text.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
