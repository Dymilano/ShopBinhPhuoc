using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
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
        item.IsRead = true;
        await _db.SaveChangesAsync();
        return View(item);
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
        }
        return RedirectToAction(nameof(Index));
    }
}
