using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Orders.Include(o => o.Items).OrderByDescending(o => o.CreatedAt).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
        var total = await query.CountAsync();
        var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Orders = list;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Status = status;
        return View();
    }

    public async Task<IActionResult> Detail(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái đơn hàng.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
