using System.Text.Json;
using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ProductsController : AdminControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ActivityLogService _activityLog;

    public ProductsController(AppDbContext db, IWebHostEnvironment env, ActivityLogService activityLog) : base(db)
    {
        _env = env;
        _activityLog = activityLog;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sản phẩm";
        var list = await Db.Products.Include(p => p.Category).Include(p => p.Brand).OrderByDescending(p => p.CreatedAt).ToListAsync();
        
        // Load categories cho dropdown xóa theo danh mục
        var allowedSlugs = AllowedCategories.Slugs;
        var categories = await Db.Categories
            .Where(c => c.IsActive && c.Slug != null && allowedSlugs.Contains(c.Slug))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        ViewBag.Categories = categories;
        
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
        // Xóa validation errors cho CategoryId và BrandId để cho phép null
        ModelState.Remove("CategoryId");
        ModelState.Remove("BrandId");
        
        // Chỉ validate Name và Price là bắt buộc
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Tên sản phẩm là bắt buộc.");
        }
        
        if (model.Price <= 0)
        {
            ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn 0.");
        }
        
        // Xử lý CategoryId và BrandId = 0 thành null
        if (model.CategoryId == 0) model.CategoryId = null;
        if (model.BrandId == 0) model.BrandId = null;
        
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
        Db.Products.Add(model);
        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("Create", "Product", model.Id, model.Name, "Thêm sản phẩm mới");
        TempData["Success"] = "Đã thêm sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa sản phẩm";
        var item = await Db.Products.FindAsync(id);
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
        var item = await Db.Products.FindAsync(id);
        if (item == null) return NotFound();
        
        // Xóa validation errors cho CategoryId và BrandId để cho phép null
        ModelState.Remove("CategoryId");
        ModelState.Remove("BrandId");
        
        // Chỉ validate Name và Price là bắt buộc
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Tên sản phẩm là bắt buộc.");
        }
        
        if (model.Price <= 0)
        {
            ModelState.AddModelError("Price", "Giá sản phẩm phải lớn hơn 0.");
        }
        
        // Xử lý CategoryId và BrandId = 0 thành null
        if (model.CategoryId == 0) model.CategoryId = null;
        if (model.BrandId == 0) model.BrandId = null;
        
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
        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("Update", "Product", item.Id, item.Name, "Cập nhật sản phẩm");
        TempData["Success"] = "Đã cập nhật sản phẩm thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await Db.Products.FindAsync(id);
        if (item != null)
        {
            await DeleteProductImages(item);
            var name = item.Name;
            Db.Products.Remove(item);
            await Db.SaveChangesAsync();
            await _activityLog.LogAsync("Delete", "Product", null, name, "Xóa sản phẩm");
            TempData["Success"] = "Đã xóa sản phẩm thành công.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected(int[] selectedIds)
    {
        if (selectedIds == null || selectedIds.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để xóa.";
            return RedirectToAction(nameof(Index));
        }

        var products = await Db.Products.Where(p => selectedIds.Contains(p.Id)).ToListAsync();
        int deletedCount = 0;

        foreach (var product in products)
        {
            await DeleteProductImages(product);
            Db.Products.Remove(product);
            deletedCount++;
        }

        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("Delete", "Product", null, $"{deletedCount} sản phẩm", $"Xóa {deletedCount} sản phẩm đã chọn");
        TempData["Success"] = $"Đã xóa thành công {deletedCount} sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteByCategory(int categoryId)
    {
        var category = await Db.Categories.FindAsync(categoryId);
        if (category == null)
        {
            TempData["Error"] = "Danh mục không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        // Lấy tất cả category con
        var childCategoryIds = await Db.Categories
            .Where(c => c.ParentId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();
        
        var allCategoryIds = new List<int> { categoryId };
        allCategoryIds.AddRange(childCategoryIds);

        var products = await Db.Products
            .Where(p => p.CategoryId.HasValue && allCategoryIds.Contains(p.CategoryId.Value))
            .ToListAsync();

        int deletedCount = 0;
        foreach (var product in products)
        {
            await DeleteProductImages(product);
            Db.Products.Remove(product);
            deletedCount++;
        }

        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("Delete", "Product", null, $"{deletedCount} sản phẩm", $"Xóa {deletedCount} sản phẩm theo danh mục: {category.Name}");
        TempData["Success"] = $"Đã xóa thành công {deletedCount} sản phẩm trong danh mục '{category.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        var products = await Db.Products.ToListAsync();
        int deletedCount = products.Count;

        foreach (var product in products)
        {
            await DeleteProductImages(product);
            Db.Products.Remove(product);
        }

        await Db.SaveChangesAsync();
        await _activityLog.LogAsync("Delete", "Product", null, $"{deletedCount} sản phẩm", $"Xóa tất cả {deletedCount} sản phẩm");
        TempData["Success"] = $"Đã xóa thành công tất cả {deletedCount} sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    private async Task DeleteProductImages(Product product)
    {
        // Xóa ảnh chính
        if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
        {
            var imagePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                try { System.IO.File.Delete(imagePath); } catch { }
            }
        }
        
        // Xóa các ảnh phụ
        var extraUrls = ParseImageUrls(product.ImageUrls);
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
    }

    private async Task LoadDropdowns()
    {
        var allowedSlugs = AllowedCategories.Slugs;
        var categories = await Db.Categories
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
        
        var brands = await Db.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
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
