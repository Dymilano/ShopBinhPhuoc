using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Cài đặt web";
        var list = await _db.SiteSettings.OrderBy(s => s.Key).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(IFormCollection form, string? newKey, string? newValue)
    {
        var keys = form.Keys.Where(k => k != null && k.StartsWith("values[") && k.EndsWith("]")).ToList();
        foreach (var k in keys)
        {
            var key = k!.Replace("values[", "").TrimEnd(']').Trim();
            if (string.IsNullOrEmpty(key)) continue;
            var val = form[k];
            var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
                _db.SiteSettings.Add(new SiteSetting { Key = key, Value = val });
            else
                setting.Value = val;
        }
        if (!string.IsNullOrWhiteSpace(newKey))
        {
            _db.SiteSettings.Add(new SiteSetting { Key = newKey.Trim(), Value = newValue });
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Đã lưu cài đặt.";
        return RedirectToAction(nameof(Index));
    }
}
