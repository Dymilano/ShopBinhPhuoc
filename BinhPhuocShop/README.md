# Bình Phước Shop - Web bán giày dép (ASP.NET Core)

Dự án web bán giày dép sử dụng **ASP.NET Core 8 MVC**, gồm:
- **Trang người dùng**: giao diện CozaStore (Trang chủ, Giới thiệu, Sản phẩm, Bài viết, Liên hệ, Giỏ hàng, Thanh toán)
- **Trang Admin**: giao diện Cleopatra Tailwind (quản lý sản phẩm, danh mục, thương hiệu, bài viết, đơn hàng, cài đặt web, tin nhắn liên hệ)

## Danh mục sản phẩm chính (5 mục)

- Giày nam
- Giày nữ
- Dép nam
- Dép nữ
- Phụ kiện

## Yêu cầu

- .NET 8 SDK
- Thư mục **cozastore-master** và **cleopatra-tailwind-1.0.0** nằm cùng cấp với thư mục **BinhPhuocShop** (để phục vụ CSS/JS/img của store và admin)

## Chạy dự án

```bash
cd BinhPhuocShop
dotnet run
```

- **Trang chủ (người dùng)**: https://localhost:5000 hoặc http://localhost:5000
- **Trang Admin**: https://localhost:5000/Admin/Dashboard hoặc http://localhost:5000/Admin/Dashboard

## Các trang web chính (store)

| Trang | URL / Mô tả |
|-------|-------------|
| Trang chủ | `/` |
| Tất cả sản phẩm | `/Products` hoặc menu Sản phẩm → Tất cả sản phẩm |
| Giày nam / Giày nữ / Dép nam / Dép nữ / Phụ kiện | `/collections/giay-nam`, `/collections/giay-nu`, … |
| Blog | `/Blog` |
| Giới thiệu | `/Pages/GioiThieu` hoặc menu Giới thiệu |
| Giỏ hàng | `/Cart` |
| Liên hệ | `/Contact` |
| Tìm kiếm | Icon kính lúp → nhập từ khóa (gửi đến `/Products?q=...`) |
| Hướng dẫn mua hàng | `/Pages/HuongDanMuaHang` (footer Hỗ trợ) |
| Chính sách vận chuyển | `/Pages/ChinhSachVanChuyen` |
| Hoàn trả / Đổi trả | `/Pages/ChinhSachDoiHang` |

## Cấu trúc

- **Areas/Admin**: Quản trị (Cleopatra Tailwind) – Tổng quan, Sản phẩm (danh sách, thêm), Danh mục, Thương hiệu, Bài viết, Đơn hàng, Liên hệ, Cài đặt web. Sidebar gọn, ảnh sản phẩm trong danh sách hiển thị nhỏ.
- **Controllers**: Home, Products, Collections, Blog, Contact, Cart, Checkout, Pages (trang store)
- **Models**: Category, Brand, Product, Post, SiteSetting, ContactMessage, Order, OrderItem
- **Data**: SQLite (file `app.db`), có thể đổi sang SQL Server trong `appsettings.json` và `Program.cs`

## Lần đầu chạy

- Database tự tạo và seed: 5 danh mục (Giày nam, Giày nữ, Dép nam, Dép nữ, Phụ kiện) và một số cài đặt web (SiteName, Phone, Email, Address).
- Vào **Admin** → **Danh mục** / **Thương hiệu** / **Sản phẩm** để thêm nội dung.
- **Cài đặt web**: Admin → Cài đặt web để sửa tên shop, số điện thoại, địa chỉ hiển thị trên trang chủ và footer.
