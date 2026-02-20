using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class PagesController : Controller
{
    private readonly AppDbContext _db;

    public PagesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> EditGioiThieu()
    {
        ViewData["Title"] = "Sửa trang Giới thiệu";
        var content = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "GioiThieuContent");
        return View(content);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGioiThieu(string? content)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "GioiThieuContent");
        if (setting == null)
        {
            _db.SiteSettings.Add(new SiteSetting { Key = "GioiThieuContent", Value = content });
        }
        else
        {
            setting.Value = content;
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Đã lưu nội dung trang Giới thiệu.";
        return RedirectToAction(nameof(EditGioiThieu));
    }
}
