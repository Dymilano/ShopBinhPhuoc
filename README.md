# Bình Phước Shop - Web bán giày dép (ASP.NET Core)

 sử dụng **ASP.NET Core 10 MVC**, gồm:
- **Trang người dùng**:Trang chủ, Giới thiệu, Sản phẩm, Bài viết, Liên hệ, Giỏ hàng, Thanh toán)
- **Trang Admin**: giao diện Admin (quản lý sản phẩm, danh mục, thương hiệu, bài viết, đơn hàng, cài đặt web, tin nhắn liên hệ, người dùng)

## Danh mục sản phẩm chính (5 mục)

- Giày nam
- Giày nữ
- Dép nam
- Dép nữ
- Phụ kiện

## Yêu cầu

- .NET 9.0 SDK (hoặc .NET 8+)
- SQL Server (LocalDB hoặc SQLEXPRESS): `DESKTOP-C14HLFU\SQLEXPRESS` ( database)

## Chạy dự án

```bash
cd BinhPhuocShop
donet build
dotnet run
```

- **Trang chủ (Web)**: http://localhost:5000
- **Trang Admin**: http://localhost:4080 hoặc http://localhost:4080/Admin
- ** Trang web địa chỉ khác và admin địa chỉ khác, chú ý.

  - **Email**: `admin@binhphuocshop.vn`( tk và mk để đăng nhập admin)
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

- **Areas/Admin**: Quản trị  – Tổng quan, Sản phẩm (danh sách, thêm, sửa, xóa), Danh mục, Thương hiệu, Bài viết, Đơn hàng, Liên hệ, Cài đặt web, Người dùng, Profile. Sidebar gọn, giao diện hiện đại.

- **Controllers**: Home, Products, Collections, Blog, Contact, Cart, Checkout, Pages, Account (trang store)

- **Models**: Category, Brand, Product, Post, SiteSetting, ContactMessage, Order, OrderItem, User, CartItem

- **Data**: SQL Server (`DESKTOP-C14HLFU\SQLEXPRESS`). Chạy `Database/CreateDatabase.sql` lần đầu để tạo database và dữ liệu mẫu

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
- SQL Server: `Server=DESKTOP-C14HLFU\SQLEXPRESS;Database=BinhPhuocShop;Trusted_Connection=True;TrustServerCertificate=True;`
- Sửa `appsettings.json` nếu dùng server/instance khác

### Session:
- Timeout: 7 ngày
- Cookie: HttpOnly, Essential

## Lưu ý

- Chạy `Database/CreateDatabase.sql` trên SQL Server trước khi chạy ứng dụng lần đầu
- Admin user mặc định: `admin@binhphuocshop.vn` / `admin123` (nên đổi sau khi deploy)
- Password admin mặc định: `admin123` (nên đổi sau khi deploy)

