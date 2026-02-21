using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ActivityLogsController : Controller
{
    private readonly AppDbContext _db;

    public ActivityLogsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int page = 1, string? action = null, string? entityType = null)
    {
        ViewData["Title"] = "Lịch sử hoạt động";
        var query = _db.ActivityLogs.OrderByDescending(l => l.CreatedAt).AsQueryable();
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(l => l.EntityType == entityType);
        var pageSize = 50;
        var total = await query.CountAsync();
        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.ActionFilter = action;
        ViewBag.EntityFilter = entityType;
        return View(list);
    }
}
