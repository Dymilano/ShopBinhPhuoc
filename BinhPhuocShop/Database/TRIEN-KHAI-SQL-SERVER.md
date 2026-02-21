# Triển khai và kết nối SQL Server - Bình Phước Shop

## Server: DESKTOP-C14HLFU\SQLEXPRESS

### Bước 1: Chạy script tạo database

1. Mở **SQL Server Management Studio**, kết nối tới `DESKTOP-C14HLFU\SQLEXPRESS` (đăng nhập `sa` + mật khẩu của bạn).
2. Mở file **`Database/CreateDatabase.sql`** trong project.
3. Chạy toàn bộ script (F5). Script sẽ:
   - Tạo database **BinhPhuocShop** (nếu chưa có)
   - Tạo đầy đủ các bảng: Categories, Brands, Products, Posts, SiteSettings, ContactMessages, Users, Orders, OrderItems, **ActivityLogs**
   - Seed: cài đặt web, 5 danh mục, tài khoản Admin (`admin@binhphuocshop.vn` / `admin123`)

### Bước 2: Cấu hình mật khẩu trong ứng dụng

1. Mở **appsettings.json** hoặc **appsettings.Development.json**.
2. Thay **`YOUR_SA_PASSWORD`** bằng mật khẩu thật của tài khoản `sa`.

Ví dụ:
```json
"DefaultConnection": "Server=DESKTOP-C14HLFU\\SQLEXPRESS;Database=BinhPhuocShop;User Id=sa;Password=MatKhauCuaBan;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Bước 3: Chạy ứng dụng

```powershell
cd c:\Users\MSI\BinhPhuocShop
dotnet run --project BinhPhuocShop\BinhPhuocShop.csproj
```

- Trang chủ: http://localhost:5188  
- Admin: http://localhost:5188/Admin/Account/Login (admin@binhphuocshop.vn / admin123)

---

## Dữ liệu hiển thị trên SQL Server

| Bảng | Nội dung |
|------|----------|
| **Products** | Sản phẩm (khi thêm/sửa/xóa từ Admin đều lưu tại đây) |
| **Categories** | Danh mục sản phẩm |
| **Brands** | Thương hiệu |
| **Users** | Tài khoản (admin + người dùng, đầy đủ Email, Name, Phone, Address, Role) |
| **Orders** | Đơn hàng khách đặt |
| **OrderItems** | Chi tiết từng đơn hàng |
| **Posts** | Bài viết blog |
| **ContactMessages** | Tin nhắn liên hệ |
| **SiteSettings** | Cài đặt website |
| **ActivityLogs** | Nhật ký mọi thao tác: admin đăng nhập, thêm/sửa/xóa sản phẩm, danh mục, thương hiệu, user, đơn hàng, bài viết, liên hệ; khách đặt đơn, gửi liên hệ |

Mọi thao tác admin và người dùng (đặt hàng, gửi liên hệ) đều được ghi vào **ActivityLogs** và có thể xem trong Admin → **Nhật ký thao tác** hoặc truy vấn trực tiếp bảng trong SSMS.
