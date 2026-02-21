-- =============================================
-- XÓA TẤT CẢ SẢN PHẨM - BinhPhuocShop
-- Chạy script này để xóa toàn bộ sản phẩm, chuẩn bị thêm lại từ đầu
-- LƯU Ý: Sẽ xóa luôn chi tiết đơn hàng (OrderItems) vì có khóa ngoại tới Products
--        Đơn hàng (Orders) vẫn giữ lại nhưng sẽ không còn dòng sản phẩm
-- =============================================

USE BinhPhuocShop;
GO

-- 1. Xóa chi tiết đơn hàng trước (do FK tới Products)
DELETE FROM dbo.OrderItems;
GO

-- 2. Xóa tất cả sản phẩm
DELETE FROM dbo.Products;
GO

PRINT N'Đã xóa toàn bộ sản phẩm và chi tiết đơn hàng. Bạn có thể thêm sản phẩm mới từ Admin.';
