using BinhPhuocShop.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class UploadController : Controller
{
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Upload ảnh cho nội dung (mô tả sản phẩm, bài viết blog). Trả về URL để chèn vào HTML.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB
    public async Task<IActionResult> ContentImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { ok = false, msg = "Chưa chọn ảnh." });
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowed.Contains(ext))
            return Json(new { ok = false, msg = "Chỉ chấp nhận ảnh: JPG, PNG, GIF, WEBP." });
        var name = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "contents");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        using (var stream = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(stream);
        var url = $"/uploads/contents/{name}";
        return Json(new { ok = true, url });
    }
}
