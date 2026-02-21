-- =============================================
-- BÌNH PHƯỚC SHOP - DATABASE ĐẦY ĐỦ SQL SERVER
-- Server: DESKTOP-C14HLFU\SQLEXPRESS
-- =============================================
--
-- Các bảng dữ liệu:
-- 1. Categories    - Danh mục sản phẩm (hiển thị danh mục)
-- 2. Brands        - Thương hiệu (hiển thị thương hiệu)
-- 3. Products      - Sản phẩm (hiển thị khi thêm mới / CRUD)
-- 4. Posts         - Bài viết blog
-- 5. SiteSettings  - Cài đặt web
-- 6. ContactMessages - Tin nhắn liên hệ
-- 7. Users         - Tài khoản (người dùng + admin, đầy đủ thông tin)
-- 8. Orders        - Đơn hàng (khách đặt)
-- 9. OrderItems    - Chi tiết đơn hàng
-- 10. ActivityLog - Nhật ký mọi thao tác admin & người dùng
--
-- =============================================

-- Tạo database
IF DB_ID(N'BinhPhuocShop') IS NULL
    CREATE DATABASE BinhPhuocShop;
GO

USE BinhPhuocShop;
GO

-- =============================================
-- 1. CATEGORIES - Danh mục sản phẩm
-- =============================================
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        Slug NVARCHAR(256) NULL,
        Description NVARCHAR(MAX) NULL,
        ParentId INT NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_Categories PRIMARY KEY (Id),
        CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES dbo.Categories(Id)
    );
END
GO

-- =============================================
-- 2. BRANDS - Thương hiệu
-- =============================================
IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        Slug NVARCHAR(256) NULL,
        Description NVARCHAR(MAX) NULL,
        LogoUrl NVARCHAR(1024) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_Brands PRIMARY KEY (Id)
    );
END
GO

-- =============================================
-- 3. PRODUCTS - Sản phẩm (hiển thị khi thêm/sửa/xóa)
-- =============================================
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        Slug NVARCHAR(256) NULL,
        Description NVARCHAR(MAX) NULL,
        ShortDescription NVARCHAR(1024) NULL,
        Price DECIMAL(18,2) NOT NULL,
        SalePrice DECIMAL(18,2) NULL,
        ImageUrl NVARCHAR(1024) NULL,
        ImageUrls NVARCHAR(MAX) NULL,
        Sku NVARCHAR(128) NULL,
        Stock INT NOT NULL DEFAULT 0,
        CategoryId INT NOT NULL,
        BrandId INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsFeatured BIT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Products PRIMARY KEY (Id),
        CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
        CONSTRAINT FK_Products_Brand FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
    );
END
GO

-- =============================================
-- 4. POSTS - Bài viết blog
-- =============================================
IF OBJECT_ID(N'dbo.Posts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Posts (
        Id INT IDENTITY(1,1) NOT NULL,
        Title NVARCHAR(512) NOT NULL,
        Slug NVARCHAR(256) NULL,
        Summary NVARCHAR(1024) NULL,
        Content NVARCHAR(MAX) NOT NULL,
        ImageUrl NVARCHAR(1024) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsPinned BIT NOT NULL DEFAULT 0,
        ViewCount INT NOT NULL DEFAULT 0,
        BlogCategory NVARCHAR(128) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        PublishedAt DATETIME2 NULL,
        CONSTRAINT PK_Posts PRIMARY KEY (Id)
    );
END
GO

-- =============================================
-- 5. SITESETTINGS - Cài đặt website
-- =============================================
IF OBJECT_ID(N'dbo.SiteSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SiteSettings (
        Id INT IDENTITY(1,1) NOT NULL,
        [Key] NVARCHAR(128) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        Description NVARCHAR(512) NULL,
        CONSTRAINT PK_SiteSettings PRIMARY KEY (Id),
        CONSTRAINT UQ_SiteSettings_Key UNIQUE ([Key])
    );
END
GO

-- =============================================
-- 6. CONTACTMESSAGES - Tin nhắn liên hệ
-- =============================================
IF OBJECT_ID(N'dbo.ContactMessages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactMessages (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        Phone NVARCHAR(64) NULL,
        Subject NVARCHAR(512) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_ContactMessages PRIMARY KEY (Id)
    );
END
GO

-- =============================================
-- 7. USERS - Tài khoản (admin + người dùng, đầy đủ thông tin)
-- =============================================
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(512) NOT NULL,
        Name NVARCHAR(256) NOT NULL,
        Phone NVARCHAR(64) NULL,
        Address NVARCHAR(512) NULL,
        Role NVARCHAR(64) NOT NULL DEFAULT N'Customer',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Users PRIMARY KEY (Id)
    );
END
GO

-- =============================================
-- 8. ORDERS - Đơn hàng (UserId: liên kết tài khoản khi khách đăng nhập đặt hàng)
-- =============================================
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId INT NULL,
        OrderCode NVARCHAR(64) NOT NULL,
        CustomerName NVARCHAR(256) NOT NULL,
        Phone NVARCHAR(64) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        Address NVARCHAR(512) NOT NULL,
        Note NVARCHAR(MAX) NULL,
        Status NVARCHAR(64) NOT NULL DEFAULT N'pending',
        TotalAmount DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_Orders PRIMARY KEY (Id),
        CONSTRAINT FK_Orders_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
END
GO

-- =============================================
-- 9. ORDERITEMS - Chi tiết đơn hàng
-- =============================================
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        Id INT IDENTITY(1,1) NOT NULL,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        ProductName NVARCHAR(256) NOT NULL,
        ProductImageUrl NVARCHAR(1024) NULL,
        Size NVARCHAR(32) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,
        CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
        CONSTRAINT FK_OrderItems_Order FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END
GO

-- =============================================
-- 10. ACTIVITYLOG - Nhật ký mọi thao tác admin & người dùng
-- =============================================
IF OBJECT_ID(N'dbo.ActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ActivityLogs (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId INT NULL,
        UserEmail NVARCHAR(256) NULL,
        UserName NVARCHAR(256) NULL,
        Role NVARCHAR(64) NULL,
        Action NVARCHAR(64) NOT NULL,
        EntityType NVARCHAR(64) NOT NULL,
        EntityId INT NULL,
        EntityName NVARCHAR(512) NULL,
        Details NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(64) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_ActivityLogs PRIMARY KEY (Id),
        CONSTRAINT FK_ActivityLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    );
    CREATE INDEX IX_ActivityLogs_CreatedAt ON dbo.ActivityLogs(CreatedAt DESC);
    CREATE INDEX IX_ActivityLogs_UserId ON dbo.ActivityLogs(UserId);
    CREATE INDEX IX_ActivityLogs_EntityType ON dbo.ActivityLogs(EntityType, EntityId);
END
GO

-- Thêm cột UserId vào Orders nếu bảng đã tồn tại (migration)
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL AND COL_LENGTH('dbo.Orders', 'UserId') IS NULL
BEGIN
    ALTER TABLE dbo.Orders ADD UserId INT NULL;
    ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id);
END
GO

-- =============================================
-- SEED - Dữ liệu mặc định
-- =============================================
IF NOT EXISTS (SELECT 1 FROM dbo.SiteSettings)
BEGIN
    SET IDENTITY_INSERT dbo.SiteSettings ON;
    INSERT INTO dbo.SiteSettings (Id, [Key], Value) VALUES
        (1, N'SiteName', N'Bình Phước Shop'),
        (2, N'SiteDescription', N'Thương hiệu chuyên kinh doanh, phân phối giày da nam. Chất lượng cao, nhiều bộ sưu tập độc đáo.'),
        (3, N'Phone', N'0984843218'),
        (4, N'Email', N'contact@binhphuocshop.vn'),
        (5, N'Address', N'123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh'),
        (6, N'WebsiteUrl', N'https://binhphuocshop.vn'),
        (7, N'HeroBackground', N'/store/images/slide-01.jpg'),
        (8, N'BannerGiayNam', N'/store/images/banner-01.jpg'),
        (9, N'BannerGiayNu', N'/store/images/banner-02.jpg'),
        (10, N'BannerDepNam', N'/store/images/banner-03.jpg');
    SET IDENTITY_INSERT dbo.SiteSettings OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    SET IDENTITY_INSERT dbo.Categories ON;
    INSERT INTO dbo.Categories (Id, Name, Slug, DisplayOrder, IsActive, CreatedAt) VALUES
        (1, N'Giày nam', N'giay-nam', 1, 1, GETUTCDATE()),
        (2, N'Giày nữ', N'giay-nu', 2, 1, GETUTCDATE()),
        (3, N'Dép nam', N'dep-nam', 3, 1, GETUTCDATE()),
        (4, N'Dép nữ', N'dep-nu', 4, 1, GETUTCDATE()),
        (5, N'Phụ kiện', N'phu-kien', 5, 1, GETUTCDATE());
    SET IDENTITY_INSERT dbo.Categories OFF;
END
GO

-- Tài khoản Admin mặc định (mật khẩu: admin123 - ứng dụng sẽ ghi đè hash khi chạy nếu cần)
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@binhphuocshop.vn')
BEGIN
    INSERT INTO dbo.Users (Email, PasswordHash, Name, Address, Role, IsActive, CreatedAt) VALUES
        (N'admin@binhphuocshop.vn',
         N'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=',
         N'Administrator',
         N'123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh',
         N'Admin',
         1,
         GETUTCDATE());
END
GO

PRINT N'Database BinhPhuocShop đã được tạo/cập nhật thành công.';
PRINT N'- Sản phẩm: bảng Products';
PRINT N'- Danh mục: bảng Categories';
PRINT N'- Thương hiệu: bảng Brands';
PRINT N'- Tài khoản (admin + user): bảng Users';
PRINT N'- Đơn hàng: bảng Orders, OrderItems';
PRINT N'- Nhật ký thao tác: bảng ActivityLogs';
