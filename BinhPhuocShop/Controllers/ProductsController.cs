using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class ProductsController : StoreControllerBase
{
    public ProductsController(AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

    public async Task<IActionResult> Index(int? categoryId, int? brandId, string? categorySlug, string? q, string sort = "newest", int page = 1, int pageSize = 12)
    {
        var query = Db.Products.Include(p => p.Category).Include(p => p.Brand).Where(p => p.IsActive);

        if (!categoryId.HasValue && !string.IsNullOrEmpty(categorySlug))
        {
            var cat = await Db.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
            if (cat != null) categoryId = cat.Id;
        }
        if (categoryId.HasValue)
        {
            // Lấy tất cả category con của category này
            var childCategoryIds = await Db.Categories
                .Where(c => c.ParentId == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();
            
            // Lọc sản phẩm thuộc category này hoặc các category con
            var allCategoryIds = new List<int> { categoryId.Value };
            allCategoryIds.AddRange(childCategoryIds);
            
            query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
        }
        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || (p.ShortDescription != null && p.ShortDescription.Contains(q)));

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Products = list;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.CategoryId = categoryId;
        ViewBag.CategorySlug = categorySlug;
        ViewBag.BrandId = brandId;
        ViewBag.Q = q;
        ViewBag.Sort = sort;
        ViewBag.Brands = await Db.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        if (categoryId.HasValue)
        {
            ViewBag.CurrentCategory = await Db.Categories.FindAsync(categoryId.Value);
            ViewData["Title"] = ViewBag.CurrentCategory?.Name ?? "Sản phẩm";
        }
        else
        {
            ViewData["Title"] = "Sản phẩm";
        }
        return View();
    }

    public async Task<IActionResult> Detail(int id)
    {
        var product = await Db.Products.Include(p => p.Category).Include(p => p.Brand).FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return NotFound();
        ViewData["Title"] = product.Name;
        ViewBag.Related = await Db.Products.Include(p => p.Category).Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != id).Take(4).ToListAsync();
        return View(product);
    }
}
