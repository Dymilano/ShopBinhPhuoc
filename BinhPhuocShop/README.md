# Bình Phước Shop - Web bán giày dép (ASP.NET Core)

Dự án web bán giày dép sử dụng **ASP.NET Core 10 MVC**, gồm:
- **Trang người dùng**: giao diện CozaStore (Trang chủ, Giới thiệu, Sản phẩm, Bài viết, Liên hệ, Giỏ hàng, Thanh toán)
- **Trang Admin**: giao diện Duralux Admin (quản lý sản phẩm, danh mục, thương hiệu, bài viết, đơn hàng, cài đặt web, tin nhắn liên hệ, người dùng)

## Danh mục sản phẩm chính (5 mục)

- Giày nam
- Giày nữ
- Dép nam
- Dép nữ
- Phụ kiện

## Yêu cầu

- .NET 10.0 SDK (hoặc .NET 8+)
- Thư mục **cozastore-master** và **cleopatra-tailwind-1.0.0** nằm cùng cấp với thư mục **BinhPhuocShop** (để phục vụ CSS/JS/img của store và admin)
- Thư mục **duralux** trong **wwwroot** (cho giao diện admin)

## Chạy dự án

```bash
cd BinhPhuocShop
dotnet run
```

- **Trang chủ (người dùng)**: http://localhost:5188
- **Trang Admin**: http://localhost:5188/Admin/Account/Login
  - **Email**: `admin@binhphuocshop.vn`
  - **Password**: `admin123`

## Các trang web chính (store)

| Trang | URL / Mô tả |
|-------|-------------|
| Trang chủ | `/` |
| Tất cả sản phẩm | `/Products` hoặc menu Sản phẩm → Tất cả sản phẩm |
| Giày nam / Giày nữ / Dép nam / Dép nữ / Phụ kiện | `/collections/giay-nam`, `/collections/giay-nu`, … |
| Blog | `/Blog` |
| Giới thiệu | `/Pages/GioiThieu` hoặc `/gioi-thieu` hoặc menu Giới thiệu |
| Giỏ hàng | `/Cart` |
| Thanh toán | `/Checkout` |
| Liên hệ | `/Contact` |
| Tìm kiếm | Icon kính lúp → nhập từ khóa (gửi đến `/Products?q=...`) |
| Hướng dẫn mua hàng | `/Pages/HuongDanMuaHang` (footer Hỗ trợ) |
| Chính sách vận chuyển | `/Pages/ChinhSachVanChuyen` |
| Hoàn trả / Đổi trả | `/Pages/ChinhSachDoiHang` |

## Cấu trúc

- **Areas/Admin**: Quản trị (Duralux Admin) – Tổng quan, Sản phẩm (danh sách, thêm, sửa, xóa), Danh mục, Thương hiệu, Bài viết, Đơn hàng, Liên hệ, Cài đặt web, Người dùng, Profile. Sidebar gọn, giao diện hiện đại.
- **Controllers**: Home, Products, Collections, Blog, Contact, Cart, Checkout, Pages, Account (trang store)
- **Models**: Category, Brand, Product, Post, SiteSetting, ContactMessage, Order, OrderItem, User, CartItem
- **Data**: SQLite (file `app.db`), có thể đổi sang SQL Server trong `appsettings.json` và `Program.cs`
- **Services**: CartService (quản lý giỏ hàng qua session)
- **Infrastructure**: AdminAuthorizationAttribute, AllowedCategories, SkipCollectionsPathConstraint

## Database Schema

### Bảng chính:
- **Categories**: Danh mục sản phẩm (hỗ trợ parent-child)
- **Brands**: Thương hiệu
- **Products**: Sản phẩm (hỗ trợ nhiều ảnh)
- **Posts**: Bài viết blog
- **Orders**: Đơn hàng
- **OrderItems**: Chi tiết đơn hàng
- **Users**: Người dùng (Admin, Manager, Customer)
- **ContactMessages**: Tin nhắn liên hệ
- **SiteSettings**: Cài đặt website

### User Roles:
- **Admin**: Quyền quản trị đầy đủ
- **Manager**: Quyền quản trị (có thể mở rộng)
- **Customer**: Người dùng thường

## Lần đầu chạy

- Database tự tạo và seed:
  - 5 danh mục mặc định (Giày nam, Giày nữ, Dép nam, Dép nữ, Phụ kiện)
  - Cài đặt web (SiteName, SiteDescription, Phone, Email, Address, WebsiteUrl)
  - User admin mặc định:
    - Email: `admin@binhphuocshop.vn`
    - Password: `admin123`
    - Role: `Admin`
    - Address: `123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh`
  - Dữ liệu mẫu sản phẩm Mulgati (nếu chưa có sản phẩm)
- Vào **Admin** → **Danh mục** / **Thương hiệu** / **Sản phẩm** để thêm nội dung.
- **Cài đặt web**: Admin → Cài đặt web để sửa tên shop, số điện thoại, địa chỉ, URL website hiển thị trên trang chủ và footer.

## Tính năng

### Trang Store:
- ✅ Trang chủ với slider, sản phẩm nổi bật, danh mục
- ✅ Danh sách sản phẩm với filter, sort, pagination
- ✅ Chi tiết sản phẩm với nhiều ảnh
- ✅ Giỏ hàng (session-based)
- ✅ Thanh toán đơn hàng
- ✅ Blog/Bài viết với phân loại
- ✅ Liên hệ
- ✅ Tìm kiếm sản phẩm
- ✅ Responsive design

### Trang Admin:
- ✅ Dashboard với thống kê
- ✅ Quản lý sản phẩm (CRUD, upload nhiều ảnh)
- ✅ Quản lý danh mục (CRUD)
- ✅ Quản lý thương hiệu (CRUD)
- ✅ Quản lý bài viết (CRUD)
- ✅ Quản lý đơn hàng (xem chi tiết, cập nhật trạng thái)
- ✅ Quản lý tin nhắn liên hệ
- ✅ Cài đặt website
- ✅ Quản lý người dùng
- ✅ Profile cá nhân
- ✅ Authorization (chỉ Admin/Manager)

## Cấu hình

### Database:
- Mặc định: SQLite (`app.db`)
- Có thể đổi sang SQL Server trong `appsettings.json`:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
  ```

### Session:
- Timeout: 7 ngày
- Cookie: HttpOnly, Essential

## Lưu ý

- Database tự động migrate khi khởi động (EnsureCreated)
- Các cột mới sẽ tự động được thêm vào bảng cũ (Address, Role, UpdatedAt trong Users)
- Admin user luôn được đảm bảo có Role = "Admin" khi khởi động
- Password admin mặc định: `admin123` (nên đổi sau khi deploy)

## License

Dự án này được phát triển cho Bình Phước Shop.
