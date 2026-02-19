using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace BinhPhuocShop.Controllers;

public class ContactController : StoreControllerBase
{
    public ContactController(AppDbContext db, BinhPhuocShop.Services.CartService cart) : base(db, cart) { }

    public IActionResult Index()
    {
        ViewData["Title"] = "Liên hệ";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactMessage model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
        {
            ModelState.AddModelError("", "Vui lòng điền đầy đủ Họ tên, Email và Nội dung.");
            return View(model);
        }
        model.CreatedAt = DateTime.UtcNow;
        Db.ContactMessages.Add(model);
        await Db.SaveChangesAsync();
        TempData["Success"] = "Gửi tin nhắn thành công. Chúng tôi sẽ phản hồi sớm.";
        return RedirectToAction(nameof(Index));
    }
}
