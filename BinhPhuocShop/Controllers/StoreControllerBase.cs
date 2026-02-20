using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class StoreControllerBase : Controller
{
    protected readonly AppDbContext Db;
    protected readonly CartService Cart;

    public StoreControllerBase(AppDbContext db, CartService cart)
    {
        Db = db;
        Cart = cart;
    }

    public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
    {
        var settings = await Db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        ViewBag.SiteName = settings.GetValueOrDefault("SiteName", "Bình Phước Shop");
        ViewBag.SiteDescription = settings.GetValueOrDefault("SiteDescription", "Giày dép chính hãng - Bình Phước Shop");
        ViewBag.Phone = settings.GetValueOrDefault("Phone", "0984843218");
        ViewBag.Email = settings.GetValueOrDefault("Email", "contact@binhphuocshop.vn");
        ViewBag.Address = settings.GetValueOrDefault("Address", "Hà Nội");
        // Lấy tất cả danh mục cha đang active, không filter theo allowedSlugs để đồng bộ với header
        ViewBag.Categories = await Db.Categories
            .Where(c => c.IsActive && c.ParentId == null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        ViewBag.Brands = await Db.Brands.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name).ToListAsync();
        ViewBag.CartCount = Cart.GetCount();
        ViewBag.CartTotal = Cart.GetTotal();
        ViewBag.CartItems = Cart.GetCart();
        ViewBag.UserId = context.HttpContext.Session.GetString("UserId");
        ViewBag.UserName = context.HttpContext.Session.GetString("UserName");
        await next();
    }
}
