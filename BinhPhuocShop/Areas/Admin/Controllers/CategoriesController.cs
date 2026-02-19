using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
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
        if (item != null && !await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static string Slugify(string text) => string.IsNullOrWhiteSpace(text) ? "" : string.Join("-", text.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
