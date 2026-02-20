using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ContactMessagesController : Controller
{
    private readonly AppDbContext _db;

    public ContactMessagesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Tin nhắn liên hệ";
        return View(await _db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await _db.ContactMessages.FindAsync(id);
        if (item == null) return NotFound();
        ViewData["Title"] = "Chi tiết liên hệ";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var item = await _db.ContactMessages.FindAsync(id);
        if (item != null)
        {
            item.IsRead = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã đánh dấu tin nhắn là đã đọc.";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.ContactMessages.FindAsync(id);
        if (item != null)
        {
            _db.ContactMessages.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa tin nhắn.";
        }
        return RedirectToAction(nameof(Index));
    }
}
