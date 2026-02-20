using System.Text.Json;
using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sản phẩm";
        var list = await _db.Products.Include(p => p.Category).Include(p => p.Brand).OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm sản phẩm";
        await LoadDropdowns();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model,
        IFormFile? imageFile, IFormFile? imageFile2, IFormFile? imageFile3, IFormFile? imageFile4, IFormFile? imageFile5)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(model);
        }
        
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        var imageList = await BuildImageListFromFiles(imageFile, imageFile2, imageFile3, imageFile4, imageFile5, null, null);
        if (imageList.Count > 0)
        {
            model.ImageUrl = imageList[0];
            if (imageList.Count > 1)
                model.ImageUrls = JsonSerializer.Serialize(imageList.Skip(1).Take(4).ToArray());
        }
        model.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(model);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Đã thêm sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa sản phẩm";
        var item = await _db.Products.FindAsync(id);
        if (item == null) return NotFound();
        await LoadDropdowns();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model,
        IFormFile? imageFile, IFormFile? imageFile2, IFormFile? imageFile3, IFormFile? imageFile4, IFormFile? imageFile5)
    {
        if (id != model.Id) return NotFound();
        var item = await _db.Products.FindAsync(id);
        if (item == null) return NotFound();
        
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(item);
        }
        
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        var existingExtra = ParseImageUrls(item.ImageUrls);
        var imageList = await BuildImageListFromFiles(imageFile, imageFile2, imageFile3, imageFile4, imageFile5, item.ImageUrl, existingExtra);
        if (imageList.Count > 0)
        {
            item.ImageUrl = imageList[0];
            item.ImageUrls = imageList.Count > 1 ? JsonSerializer.Serialize(imageList.Skip(1).Take(4).ToArray()) : null;
        }
        item.Name = model.Name;
        item.Slug = model.Slug;
        item.Description = model.Description;
        item.ShortDescription = model.ShortDescription;
        item.Price = model.Price;
        item.SalePrice = model.SalePrice;
        item.Sku = model.Sku;
        item.Stock = model.Stock;
        item.CategoryId = model.CategoryId;
        item.BrandId = model.BrandId;
        item.IsActive = model.IsActive;
        item.IsFeatured = model.IsFeatured;
        item.DisplayOrder = model.DisplayOrder;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Đã cập nhật sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Products.FindAsync(id);
        if (item != null)
        {
            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(item.ImageUrl) && item.ImageUrl.StartsWith("/uploads/"))
            {
                var imagePath = Path.Combine(_env.WebRootPath, item.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    try { System.IO.File.Delete(imagePath); } catch { }
                }
            }
            
            var extraUrls = ParseImageUrls(item.ImageUrls);
            foreach (var url in extraUrls)
            {
                if (!string.IsNullOrEmpty(url) && url.StartsWith("/uploads/"))
                {
                    var imagePath = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        try { System.IO.File.Delete(imagePath); } catch { }
                    }
                }
            }
            
            _db.Products.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Đã xóa sản phẩm thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdowns()
    {
        var allowedSlugs = AllowedCategories.Slugs;
        var categories = await _db.Categories
            .Where(c => c.IsActive && c.Slug != null && allowedSlugs.Contains(c.Slug))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        // Đảm bảo SelectList không null
        if (categories != null && categories.Any())
        {
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }
        else
        {
            ViewBag.Categories = new SelectList(new List<Category>(), "Id", "Name");
        }
        
        var brands = await _db.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        if (brands != null && brands.Any())
        {
            ViewBag.Brands = new SelectList(brands, "Id", "Name");
        }
        else
        {
            ViewBag.Brands = new SelectList(new List<Brand>(), "Id", "Name");
        }
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

    /// <summary>Build list of up to 5 image URLs from 5 file inputs; khi không có file mới thì giữ ảnh cũ (cho Edit).</summary>
    private async Task<List<string>> BuildImageListFromFiles(
        IFormFile? f1, IFormFile? f2, IFormFile? f3, IFormFile? f4, IFormFile? f5,
        string? existingMain, List<string>? existingExtra)
    {
        existingExtra ??= new List<string>();
        var list = new List<string>();
        if (f1 != null) list.Add(await SaveFile(f1, "products"));
        else if (!string.IsNullOrEmpty(existingMain)) list.Add(existingMain);
        for (int i = 0; i < 4; i++)
        {
            var f = i == 0 ? f2 : i == 1 ? f3 : i == 2 ? f4 : f5;
            if (f != null) list.Add(await SaveFile(f, "products"));
            else if (existingExtra.Count > i && !string.IsNullOrEmpty(existingExtra[i])) list.Add(existingExtra[i]);
        }
        return list.Take(5).ToList();
    }

    private static List<string> ParseImageUrls(string? imageUrls)
    {
        if (string.IsNullOrWhiteSpace(imageUrls)) return new List<string>();
        try
        {
            var arr = JsonSerializer.Deserialize<string[]>(imageUrls);
            return arr?.Where(u => !string.IsNullOrEmpty(u)).ToList() ?? new List<string>();
        }
        catch { return new List<string>(); }
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return string.Join("-", text.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
