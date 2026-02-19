using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class BrandsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public BrandsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Thương hiệu";
        return View(await _db.Brands.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm thương hiệu";
        return View(new Brand());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Brand model, IFormFile? logoFile)
    {
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        if (logoFile != null) model.LogoUrl = await SaveFile(logoFile, "brands");
        _db.Brands.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa thương hiệu";
        var item = await _db.Brands.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Brand model, IFormFile? logoFile)
    {
        if (id != model.Id) return NotFound();
        var item = await _db.Brands.FindAsync(id);
        if (item == null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        if (logoFile != null) item.LogoUrl = await SaveFile(logoFile, "brands");
        item.Name = model.Name;
        item.Slug = model.Slug;
        item.Description = model.Description;
        item.DisplayOrder = model.DisplayOrder;
        item.IsActive = model.IsActive;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Brands.FindAsync(id);
        if (item != null)
        {
            _db.Brands.Remove(item);
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
