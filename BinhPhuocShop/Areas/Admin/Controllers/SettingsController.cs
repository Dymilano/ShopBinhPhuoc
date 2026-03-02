using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using BinhPhuocShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorization]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SettingsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Cài đặt web";
        var list = await _db.SiteSettings.OrderBy(s => s.Key).ToListAsync();
        
        // Lấy tất cả banner/ảnh từ thư mục uploads (không phải sản phẩm)
        var bannersPath = Path.Combine(_env.WebRootPath, "uploads", "banners");
        var postsPath = Path.Combine(_env.WebRootPath, "uploads", "posts");
        var allImages = new List<dynamic>();
        
        if (Directory.Exists(bannersPath))
        {
            var bannerFiles = Directory.GetFiles(bannersPath, "*.*", SearchOption.AllDirectories)
                .Where(f => new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".webm" }.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => new { 
                    Url = f.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                    Name = Path.GetFileName(f),
                    Type = "banner",
                    IsVideo = new[] { ".mp4", ".webm" }.Contains(Path.GetExtension(f).ToLowerInvariant()),
                    FullPath = f
                });
            allImages.AddRange(bannerFiles);
        }
        
        if (Directory.Exists(postsPath))
        {
            var postFiles = Directory.GetFiles(postsPath, "*.*", SearchOption.AllDirectories)
                .Where(f => new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => new { 
                    Url = f.Replace(_env.WebRootPath, "").Replace("\\", "/"),
                    Name = Path.GetFileName(f),
                    Type = "post",
                    IsVideo = false,
                    FullPath = f
                });
            allImages.AddRange(postFiles);
        }
        
        ViewBag.AllImages = allImages.OrderByDescending(i => i.FullPath).ToList();
        
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
        TempData["Success"] = "Đã lưu cài đặt thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBanner(IFormFile? bannerFile, string? bannerType, string? bannerLink)
    {
        if (bannerFile == null || bannerFile.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file để tải lên.";
            return RedirectToAction(nameof(Index));
        }

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "banners");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{bannerType ?? "home"}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(bannerFile.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await bannerFile.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/banners/{fileName}";
        var key = $"Banner{bannerType?.Substring(0, 1).ToUpper()}{bannerType?.Substring(1) ?? "Home"}";
        
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            _db.SiteSettings.Add(new SiteSetting { Key = key, Value = relativePath });
        }
        else
        {
            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(setting.Value))
            {
                var oldPath = Path.Combine(_env.WebRootPath, setting.Value.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }
            setting.Value = relativePath;
        }

        if (!string.IsNullOrWhiteSpace(bannerLink))
        {
            var linkKey = $"{key}Link";
            var linkSetting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == linkKey);
            if (linkSetting == null)
                _db.SiteSettings.Add(new SiteSetting { Key = linkKey, Value = bannerLink });
            else
                linkSetting.Value = bannerLink;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tải lên banner thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBanner(string? bannerKey)
    {
        if (string.IsNullOrWhiteSpace(bannerKey))
            return RedirectToAction(nameof(Index));

        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == bannerKey);
        if (setting != null && !string.IsNullOrEmpty(setting.Value))
        {
            var filePath = Path.Combine(_env.WebRootPath, setting.Value.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            
            _db.SiteSettings.Remove(setting);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa banner thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !System.IO.File.Exists(imagePath))
        {
            TempData["Error"] = "File không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            System.IO.File.Delete(imagePath);
            TempData["Success"] = "Đã xóa ảnh thành công.";
        }
        catch
        {
            TempData["Error"] = "Lỗi khi xóa ảnh.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ManageHomeImages()
    {
        ViewData["Title"] = "Quản lý ảnh trang chủ";
        
        // Lấy 4 ảnh chính từ settings
        var heroBg = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "HeroBackground");
        var bannerGiayNam = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "BannerGiayNam");
        var bannerGiayNu = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "BannerGiayNu");
        var bannerDepNam = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == "BannerDepNam");
        
        ViewBag.HeroBackground = heroBg?.Value ?? "/hexashop/assets/images/left-banner-image.jpg";
        ViewBag.BannerGiayNam = bannerGiayNam?.Value ?? "/hexashop/assets/images/baner-right-image-01.jpg";
        ViewBag.BannerGiayNu = bannerGiayNu?.Value ?? "/hexashop/assets/images/baner-right-image-02.jpg";
        ViewBag.BannerDepNam = bannerDepNam?.Value ?? "/hexashop/assets/images/baner-right-image-03.jpg";
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadHomeImage(string imageType, IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file ảnh.";
            return RedirectToAction(nameof(Index));
        }

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "home-images");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext))
        {
            TempData["Error"] = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP).";
            return RedirectToAction(nameof(Index));
        }

        var fileName = $"{imageType}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/home-images/{fileName}";
        
        // Lưu vào settings
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == imageType);
        if (setting == null)
        {
            _db.SiteSettings.Add(new SiteSetting { Key = imageType, Value = relativePath });
        }
        else
        {
            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(setting.Value) && setting.Value.StartsWith("/uploads/"))
            {
                var oldPath = Path.Combine(_env.WebRootPath, setting.Value.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
            }
            setting.Value = relativePath;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật ảnh {GetImageTypeName(imageType)} thành công.";
        return RedirectToAction(nameof(Index));
    }

    private string GetImageTypeName(string imageType)
    {
        return imageType switch
        {
            "HeroBackground" => "Ảnh background",
            "BannerGiayNam" => "Ảnh giày nam",
            "BannerGiayNu" => "Ảnh giày nữ",
            "BannerDepNam" => "Ảnh dép nam",
            _ => "Ảnh"
        };
    }
}
