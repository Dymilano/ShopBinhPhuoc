using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class CategoriesController : Controller
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Danh mục";
        var allowedSlugs = AllowedCategories.Slugs;
        var list = await _db.Categories
            .Include(c => c.Parent)
            .Where(c => c.Slug != null && allowedSlugs.Contains(c.Slug))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        
        // Đếm số sản phẩm theo từng danh mục
        var productCounts = new Dictionary<int, int>();
        foreach (var category in list)
        {
            var childIds = await _db.Categories.Where(c => c.ParentId == category.Id).Select(c => c.Id).ToListAsync();
            var allIds = new List<int> { category.Id };
            allIds.AddRange(childIds);
            var count = await _db.Products.CountAsync(p => p.CategoryId.HasValue && allIds.Contains(p.CategoryId.Value));
            productCounts[category.Id] = count;
        }
        ViewBag.ProductCounts = productCounts;
        
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm danh mục";
        var allowedSlugs = AllowedCategories.Slugs;
        ViewBag.Parents = new SelectList(
            await _db.Categories.Where(c => c.ParentId == null && c.Slug != null && allowedSlugs.Contains(c.Slug)).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");
        ViewBag.AllowedSlugs = allowedSlugs;
        return View(new Category());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        if (!AllowedCategories.IsAllowed(model.Slug))
        {
            ModelState.AddModelError("Slug", "Chỉ được dùng 5 danh mục: giay-nam, giay-nu, dep-nam, dep-nu, phu-kien.");
            ViewBag.Parents = new SelectList(await _db.Categories.Where(c => c.ParentId == null).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View(model);
        }
        if (await _db.Categories.AnyAsync(c => c.Slug == model.Slug))
        {
            ModelState.AddModelError("Slug", "Slug này đã tồn tại.");
            ViewBag.Parents = new SelectList(await _db.Categories.Where(c => c.ParentId == null).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View(model);
        }
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Sửa danh mục";
        var item = await _db.Categories.FindAsync(id);
        if (item == null) return NotFound();
        var allowedSlugs = AllowedCategories.Slugs;
        if (item.Slug == null || !allowedSlugs.Contains(item.Slug))
            return NotFound();
        ViewBag.Parents = new SelectList(
            await _db.Categories.Where(c => c.ParentId == null && c.Id != id && c.Slug != null && allowedSlugs.Contains(c.Slug)).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", item.ParentId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        if (id != model.Id) return NotFound();
        var item = await _db.Categories.FindAsync(id);
        if (item == null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.Slug)) model.Slug = Slugify(model.Name);
        if (!AllowedCategories.IsAllowed(model.Slug))
        {
            ModelState.AddModelError("Slug", "Chỉ được dùng 5 danh mục: giay-nam, giay-nu, dep-nam, dep-nu, phu-kien.");
            ViewBag.Parents = new SelectList(await _db.Categories.Where(c => c.ParentId == null && c.Id != id).OrderBy(c => c.Name).ToListAsync(), "Id", "Name", item.ParentId);
            return View(model);
        }
        item.Name = model.Name;
        item.Slug = model.Slug;
        item.Description = model.Description;
        item.ParentId = model.ParentId;
        item.DisplayOrder = model.DisplayOrder;
        item.IsActive = model.IsActive;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Categories.FindAsync(id);
        if (item != null && !await _db.Products.AnyAsync(p => p.CategoryId.HasValue && p.CategoryId.Value == id))
        {
            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa danh mục thành công.";
        }
        else
        {
            TempData["Error"] = "Không thể xóa danh mục này vì đã có sản phẩm.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected(string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một danh mục để xóa.";
            return RedirectToAction(nameof(Index));
        }
        
        var idList = ids.Split(',').Select(id => int.TryParse(id, out var i) ? i : 0).Where(i => i > 0).ToList();
        var categories = await _db.Categories.Where(c => idList.Contains(c.Id)).ToListAsync();
        var deletedCount = 0;
        
        foreach (var category in categories)
        {
            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId.HasValue && p.CategoryId.Value == category.Id);
            if (!hasProducts)
            {
                _db.Categories.Remove(category);
                deletedCount++;
            }
        }
        
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã xóa {deletedCount} danh mục thành công.";
        if (deletedCount < categories.Count)
        {
            TempData["Error"] = $"Có {categories.Count - deletedCount} danh mục không thể xóa vì đã có sản phẩm.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        var allowedSlugs = AllowedCategories.Slugs;
        var categories = await _db.Categories
            .Where(c => c.Slug != null && allowedSlugs.Contains(c.Slug))
            .ToListAsync();
        
        var deletedCount = 0;
        foreach (var category in categories)
        {
            var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId.HasValue && p.CategoryId.Value == category.Id);
            if (!hasProducts)
            {
                _db.Categories.Remove(category);
                deletedCount++;
            }
        }
        
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã xóa {deletedCount} danh mục thành công.";
        if (deletedCount < categories.Count)
        {
            TempData["Error"] = $"Có {categories.Count - deletedCount} danh mục không thể xóa vì đã có sản phẩm.";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryProducts(int categoryId)
    {
        var category = await _db.Categories.FindAsync(categoryId);
        if (category == null)
        {
            TempData["Error"] = "Danh mục không tồn tại.";
            return RedirectToAction(nameof(Index));
        }
        
        // Lấy tất cả category con
        var childIds = await _db.Categories.Where(c => c.ParentId == categoryId).Select(c => c.Id).ToListAsync();
        var allIds = new List<int> { categoryId };
        allIds.AddRange(childIds);
        
        // Xóa tất cả sản phẩm thuộc danh mục này và các danh mục con
        var products = await _db.Products.Where(p => p.CategoryId.HasValue && allIds.Contains(p.CategoryId.Value)).ToListAsync();
        var deletedCount = products.Count;
        
        // Xóa ảnh sản phẩm
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        foreach (var product in products)
        {
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(env.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    try { System.IO.File.Delete(imagePath); } catch { }
                }
            }
        }
        
        _db.Products.RemoveRange(products);
        await _db.SaveChangesAsync();
        
        TempData["Success"] = $"Đã xóa {deletedCount} sản phẩm thuộc danh mục '{category.Name}' thành công.";
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string text) => string.IsNullOrWhiteSpace(text) ? "" : string.Join("-", text.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
