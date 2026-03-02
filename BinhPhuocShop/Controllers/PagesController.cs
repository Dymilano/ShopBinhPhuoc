using BinhPhuocShop.Data;
using BinhPhuocShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Controllers;

public class PagesController : StoreControllerBase
{
    public PagesController(AppDbContext db, CartService cart) : base(db, cart) { }

    public async Task<IActionResult> GioiThieu()
    {
        ViewData["Title"] = "Giới thiệu";
        
        // Lấy thông tin các danh mục sản phẩm chính
        var productCategories = await Db.Categories
            .Where(c => c.IsActive && c.Slug != null && 
                (c.Slug == "giay-nam" || c.Slug == "giay-nu" || c.Slug == "dep-nam" || c.Slug == "dep-nu"))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
        
        // Đếm số sản phẩm theo từng danh mục
        var categoryProductCounts = new Dictionary<string, int>();
        foreach (var cat in productCategories)
        {
            var childIds = await Db.Categories.Where(c => c.ParentId == cat.Id).Select(c => c.Id).ToListAsync();
            var ids = new List<int> { cat.Id }.Union(childIds).ToList();
            var count = await Db.Products.CountAsync(p => p.CategoryId.HasValue && ids.Contains(p.CategoryId.Value) && p.IsActive);
            categoryProductCounts[cat.Slug ?? ""] = count;
        }
        
        ViewBag.ProductCategories = productCategories;
        ViewBag.CategoryProductCounts = categoryProductCounts;
        
        return View();
    }

    public IActionResult ChinhSachBaoHanh()
    {
        ViewData["Title"] = "Chính sách bảo hành";
        return View();
    }

    public IActionResult ChinhSachDoiHang()
    {
        ViewData["Title"] = "Chính sách đổi hàng";
        return View();
    }

    public IActionResult PhuongThucThanhToan()
    {
        ViewData["Title"] = "Phương thức thanh toán";
        return View();
    }

    public IActionResult DieuKhoanDichVu()
    {
        ViewData["Title"] = "Điều khoản dịch vụ";
        return View();
    }

    public IActionResult ChinhSachVanChuyen()
    {
        ViewData["Title"] = "Chính sách vận chuyển";
        return View();
    }

    public IActionResult HuongDanMuaHang()
    {
        ViewData["Title"] = "Hướng dẫn mua hàng";
        return View();
    }

    public IActionResult HeThongCuaHang()
    {
        ViewData["Title"] = "Hệ thống cửa hàng";
        return View();
    }

    public IActionResult ChinhSachQuyenRiengTu()
    {
        ViewData["Title"] = "Chính sách bảo mật & quyền riêng tư";
        return View();
    }

    public IActionResult ChinhSachKhachHangThanThiet()
    {
        ViewData["Title"] = "Chính sách khách hàng thân thiết";
        return View();
    }

    public IActionResult Show(string slug)
    {
        var (viewName, title, pageTitle) = slug?.ToLowerInvariant() switch
        {
            "gioi-thieu" => ("GioiThieu", "Giới thiệu", "MULGATI - Thương hiệu giày da cao cấp đến từ Nga"),
            "chinh-sach-bao-hanh" => ("ChinhSachBaoHanh", "Chính sách bảo hành", "Chính sách bảo hành"),
            "chinh-sach-doi-hang" => ("ChinhSachDoiHang", "Chính sách đổi hàng", "Chính sách đổi hàng"),
            "phuong-thuc-thanh-toan" => ("PhuongThucThanhToan", "Phương thức thanh toán", "Phương thức thanh toán"),
            "dieu-khoan-dich-vu" => ("DieuKhoanDichVu", "Điều khoản dịch vụ", "Điều khoản dịch vụ"),
            "chinh-sach-van-chuyen" => ("ChinhSachVanChuyen", "Chính sách vận chuyển", "Chính sách vận chuyển"),
            "huong-dan-mua-hang" => ("HuongDanMuaHang", "Hướng dẫn mua hàng", "Hướng dẫn mua hàng"),
            "he-thong-cua-hang-1" or "he-thong-cua-hang" => ("HeThongCuaHang", "Hệ thống cửa hàng", "Hệ thống cửa hàng"),
            "chinh-sach-quyen-rieng-tu" => ("ChinhSachQuyenRiengTu", "Chính sách bảo mật & quyền riêng tư", "Chính sách bảo mật & quyền riêng tư"),
            "chinh-sach-khach-hang-than-thiet" => ("ChinhSachKhachHangThanThiet", "Chính sách khách hàng thân thiết", "Chính sách khách hàng thân thiết"),
            "lien-he" => (null, null, null), // xử lý riêng: redirect
            _ => (null, null, null)
        };
        if (viewName == null)
        {
            if (slug?.ToLowerInvariant() == "lien-he")
                return RedirectToAction("Index", "Contact");
            return NotFound();
        }
        ViewData["Title"] = title;
        ViewData["PageTitle"] = pageTitle;
        return View(viewName);
    }
}
