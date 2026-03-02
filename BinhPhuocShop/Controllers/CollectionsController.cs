using Microsoft.AspNetCore.Mvc;

namespace BinhPhuocShop.Controllers;

/// <summary>
/// Chỉ dùng attribute route để tránh AmbiguousMatchException (một endpoint duy nhất cho /collections/{slug}).
/// </summary>
[Route("collections")]
public class CollectionsController : ProductsController
{
    public CollectionsController(BinhPhuocShop.Data.AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Index(string slug, int? brandId, string? q, string sort = "newest", int page = 1, int pageSize = 16)
    {
        return await base.Index(null, brandId, slug, q, sort, page, pageSize);
    }
}
