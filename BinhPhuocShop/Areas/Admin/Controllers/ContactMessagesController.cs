using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class ContactMessagesController : AdminControllerBase
{
    public ContactMessagesController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Tin nhắn liên hệ";
        return View(await Db.ContactMessages.OrderByDescending(m => m.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Detail(int id)
    {
        var item = await Db.ContactMessages.FindAsync(id);
        if (item == null) return NotFound();
        ViewData["Title"] = "Chi tiết liên hệ";
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var item = await Db.ContactMessages.FindAsync(id);
        if (item != null)
        {
            item.IsRead = true;
            await Db.SaveChangesAsync();
            TempData["Success"] = "Đã đánh dấu tin nhắn là đã đọc.";
        }
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await Db.ContactMessages.FindAsync(id);
        if (item != null)
        {
            Db.ContactMessages.Remove(item);
            await Db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa tin nhắn.";
        }
        return RedirectToAction(nameof(Index));
    }
}
