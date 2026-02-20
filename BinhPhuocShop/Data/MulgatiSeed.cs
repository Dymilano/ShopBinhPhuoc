using System.Security.Cryptography;
using System.Text;
using BinhPhuocShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Data;

public static class MulgatiSeed
{

    public static async Task SeedAsync(AppDbContext db)
    {
        // Đảm bảo user admin luôn tồn tại với password đúng (chạy trước khi check products)
        var adminEmail = "admin@binhphuocshop.vn";
        var adminPassword = "admin123";
        var adminPasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(adminPassword)));
        
        var adminUser = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser == null)
        {
            db.Users.Add(new User
            {
                Email = adminEmail,
                Name = "Admin",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                Address = "123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else
        {
            // Cập nhật password hash, Role và Address nếu user đã tồn tại
            if (adminUser.PasswordHash != adminPasswordHash || adminUser.Role != "Admin")
            {
                adminUser.PasswordHash = adminPasswordHash;
                adminUser.Role = "Admin";
                adminUser.Address = adminUser.Address ?? "123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh";
                adminUser.IsActive = true;
                adminUser.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        if (await db.Products.AnyAsync()) return;

        var brandMulgati = await db.Brands.FirstOrDefaultAsync(b => b.Slug == "mulgati");
        if (brandMulgati == null)
        {
            brandMulgati = new Brand { Name = "Mulgati", Slug = "mulgati", IsActive = true, DisplayOrder = 1 };
            db.Brands.Add(brandMulgati);
            await db.SaveChangesAsync();
        }

        var catData = new[] {
            ("Giày nam", "giay-nam", 1),
            ("Giày nữ", "giay-nu", 2),
            ("Dép nam", "dep-nam", 3),
            ("Dép nữ", "dep-nu", 4),
            ("Phụ kiện", "phu-kien", 5),
        };
        foreach (var (name, slug, order) in catData)
        {
            if (!await db.Categories.AnyAsync(c => c.Slug == slug))
            {
                db.Categories.Add(new Category { Name = name, Slug = slug, DisplayOrder = order, IsActive = true });
                await db.SaveChangesAsync();
            }
        }

        var products = new[] {
            ("Giày nam Mulgati Leather Sneaker HX37A", "HX37A", 2565000, 2850000, "giay-nam", "https://product.hstatic.net/200000410665/product/hx37a-trang_aa94a948c81c4341b9443363cd3e3011.jpg"),
            ("Giày nam Mulgati Slip on - 3303-3", "3303-3", 2340000, 2600000, "giay-nam", "https://product.hstatic.net/200000410665/product/giay-luoi-3303-3-1_26e6d57828184473ab7eddaabf6b3e58.jpg"),
            ("Giày nam Mulgati Penny Mules Shoes A16739", "A16739", 2475000, 2750000, "giay-nam", "https://product.hstatic.net/200000410665/product/a16739_5c4c819792b04f24a4f5e4b1a0f3d2c1.jpg"),
            ("Giày nam Mulgati Horsebit Loafer M62136", "M62136", 3015000, 3350000, "giay-nam", "https://product.hstatic.net/200000410665/product/m62136_2a3b4c5d6e7f8090a1b2c3d4e5f6a7b8.jpg"),
            ("Giày nữ Mulgati Driving Loafer F1816", "F1816", 2682000, 2980000, "giay-nu", "https://product.hstatic.net/200000410665/product/f1816_1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e.jpg"),
            ("Giày nam Mulgati Horsebit Loafer A258C-A2", "A258C-A2", 3150000, 3500000, "giay-nam", "https://product.hstatic.net/200000410665/product/a258c_7f8e9d0c1b2a3948e5f6a7b8c9d0e1f.jpg"),
            ("Dép nam Mulgati 7642AT", "7642AT", 2673000, 2970000, "dep-nam", "https://product.hstatic.net/200000410665/product/7642at_9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d.jpg"),
            ("Dép nữ Mulgati MJ2025-16", "MJ2025-16", 2565000, 2850000, "dep-nu", "https://product.hstatic.net/200000410665/product/mj2025_3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f.jpg"),
            ("Giày nam Chelsea Boots Mulgati F708-5", "F708-5", 3195000, 3550000, "giay-nam", "https://product.hstatic.net/200000410665/product/f708_5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b.jpg"),
            ("Giày nam Mulgati Horsebit Loafer F268-8", "F268-8", 3195000, 3550000, "giay-nam", "https://product.hstatic.net/200000410665/product/f268_1c2d3e4f5a6b7c8d9e0f1a2b3c4d5e6f.jpg"),
            ("Giày nam Mulgati Horsebit Loafer F662-A17", "F662-A17", 2862000, 3180000, "giay-nam", "https://product.hstatic.net/200000410665/product/f662_7g8h9i0j1k2l3m4n5o6p7q8r9s0t1u.jpg"),
            ("Giày nam Mulgati Penny Loafer F268-6", "F268-6", 3500000, 3500000, "giay-nam", "https://product.hstatic.net/200000410665/product/f2686_2v3w4x5y6z7a8b9c0d1e2f3g4h5i6j.jpg"),
            ("Giày nữ Mulgati Penny Loafer SP22365A", "SP22365A", 5184000, 5760000, "giay-nu", "https://product.hstatic.net/200000410665/product/sp22365_7k8l9m0n1o2p3q4r5s6t7u8v9w0x1y.jpg"),
            ("Dép nam Mulgati Penny Moccasin F43806", "F43806", 2682000, 2980000, "dep-nam", "https://product.hstatic.net/200000410665/product/f43806_2z3a4b5c6d7e8f9g0h1i2j3k4l5m6n.jpg"),
            ("Giày nam Mulgati Penny Loafer SP17553", "SP17553", 5310000, 5900000, "giay-nam", "https://product.hstatic.net/200000410665/product/sp17553_7o8p9q0r1s2t3u4v5w6x7y8z9a0b1c.jpg"),
            ("Giày nam Mulgati Leather Sneaker 22603", "22603", 2565000, 2850000, "giay-nam", "https://product.hstatic.net/200000410665/product/22603_2d3e4f5g6h7i8j9k0l1m2n3o4p5q6r.jpg"),
            ("Giày nữ Mulgati Classic Sneaker JJ47A", "JJ47A", 2682000, 2980000, "giay-nu", "https://product.hstatic.net/200000410665/product/jj47a_7s8t9u0v1w2x3y4z5a6b7c8d9e0f1g.jpg"),
            ("Giày nam Mulgati Moc Toe Derby 1026A", "1026A", 3950000, 3950000, "giay-nam", "https://product.hstatic.net/200000410665/product/1026a_2h3i4j5k6l7m8n9o0p1q2r3s4t5u6v.jpg"),
            ("Thắt lưng nam Mulgati Brown CN0042052", "CN0042052", 890000, 890000, "phu-kien", "https://product.hstatic.net/200000410665/product/cn004_7w8x9y0z1a2b3c4d5e6f7g8h9i0j1k.jpg"),
            ("Túi da nam Mulgati Leather Clutch TAB7710", "TAB7710", 1850000, 1850000, "phu-kien", "https://product.hstatic.net/200000410665/product/tab7710_2l3m4n5o6p7q8r9s0t1u2v3w4x5y6z.jpg"),
        };

        var catBySlug = await db.Categories.ToDictionaryAsync(c => c.Slug ?? "", c => c);
        foreach (var (name, sku, price, comparePrice, catSlug, imgUrl) in products)
        {
            var catId = catBySlug.TryGetValue(catSlug, out var cat) ? cat.Id : (await db.Categories.FirstAsync()).Id;
            var slug = $"{catSlug}-{sku.ToLower()}".Replace(" ", "-").Replace("à", "a").Replace("á", "a").Replace("ạ", "a").Replace("ả", "a").Replace("ã", "a")
                .Replace("è", "e").Replace("é", "e").Replace("ẹ", "e").Replace("ẻ", "e").Replace("ẽ", "e")
                .Replace("ì", "i").Replace("í", "i").Replace("ị", "i").Replace("ỉ", "i").Replace("ĩ", "i")
                .Replace("ò", "o").Replace("ó", "o").Replace("ọ", "o").Replace("ỏ", "o").Replace("õ", "o")
                .Replace("ù", "u").Replace("ú", "u").Replace("ụ", "u").Replace("ủ", "u").Replace("ũ", "u")
                .Replace("ỳ", "y").Replace("ý", "y").Replace("ỵ", "y").Replace("ỷ", "y").Replace("ỹ", "y")
                .Replace("đ", "d");
            db.Products.Add(new Product
            {
                Name = name,
                Slug = slug,
                Sku = sku,
                Price = comparePrice,
                SalePrice = price < comparePrice ? price : null,
                ImageUrl = imgUrl,
                ShortDescription = $"Mã sản phẩm: {sku}. Chất liệu da cao cấp, thiết kế tinh tế.",
                Description = $"<p>Mã sản phẩm: {sku}</p><p>Chất liệu da cao cấp nhập khẩu. Sản xuất: Guangzhou theo tiêu chuẩn chất lượng châu Âu (EU Standard).</p><p>Sản phẩm chính hãng Mulgati - Thương hiệu giày da đến từ Nga.</p>",
                CategoryId = catId,
                BrandId = brandMulgati.Id,
                Stock = 50,
                IsActive = true,
                IsFeatured = true,
                DisplayOrder = 0,
            });
        }
        await db.SaveChangesAsync();

        var blogCats = new[] {
            ("phong-cach", "Phong cách", new[] {
                ("Các loại giày nam", "cac-loai-giay-nam", "Tổng hợp các loại giày nam phổ biến và cách chọn lựa phù hợp."),
                ("Giày Derby là gì", "giay-derby-la-gi", "Giày Derby - phong cách cổ điển không bao giờ lỗi mốt."),
                ("Giày Oxford là gì", "giay-oxford-la-gi", "Khám phá các loại giày Oxford phổ biến."),
                ("Cách đo size giày nam Mulgati", "cach-do-size-giay-nam-mulgati", "Hướng dẫn chi tiết cách đo và chọn size giày nam."),
                ("Giày lười nam di với quần jean", "giay-luoi-nam-di-voi-quan-jean", "Bí quyết phối đồ giày lười với quần jean."),
            }),
            ("tin-tuc-moi-ve-mulgati", "Tin tức - Khuyến mãi", new[] {
                ("Black Friday Sale - Ưu đãi lớn nhất năm", "black-friday-sale", "Giảm giá lên đến 70% nhân dịp Black Friday."),
                ("Mulgati tri ân khách hàng thân thiết", "mulgati-tri-an-khach-hang", "Chương trình tri ân đặc biệt dành cho khách hàng thân thiết."),
                ("Khai trương showroom Mulgati tại Đà Nẵng", "khai-truong-showroom-da-nang", "Showroom mới tại Vincom Plaza Ngô Quyền, Đà Nẵng."),
                ("Ưu đãi đặc biệt mừng 30/4 - 1/5", "uu-dai-30-04-01-05", "Combo giày Mulgati giá sốc nhân dịp lễ."),
            }),
            ("bao-chi", "Báo chí", new[] {
                ("Mulgati được vinh danh Top 100 sản phẩm", "mulgati-top-100", "HTV1 - Mulgati được vinh danh Top 100 sản phẩm dịch vụ tốt nhất."),
                ("Top 10 thương hiệu nổi tiếng Việt Nam", "top-10-thuong-hieu", "Mulgati đón nhận danh hiệu Top 10 thương hiệu nổi tiếng 2023."),
                ("Sức hút từ giày da phong cách Hoàng gia", "suc-hut-giay-da-hoang-gia", "Phong cách Hoàng gia đang được yêu thích tại Việt Nam."),
            }),
            ("goc-review", "Góc Review", new[] {
                ("Review giày Mulgati Leather Sneaker HX37A", "review-hx37a", "Trải nghiệm thực tế đôi giày thể thao nam bán chạy nhất."),
                ("Review giày lười Penny Loafer Mulgati", "review-penny-loafer", "Đánh giá chi tiết giày lười nam cao cấp."),
            }),
        };

        foreach (var (catSlug, catName, posts) in blogCats)
        {
            foreach (var (title, slug, summary) in posts)
            {
                if (await db.Posts.AnyAsync(p => p.Slug == slug)) continue;
                db.Posts.Add(new Post
                {
                    Title = title,
                    Slug = slug,
                    Summary = summary,
                    Content = $"<p>{summary}</p><p>Nội dung chi tiết đang được cập nhật. Vui lòng quay lại sau.</p>",
                    BlogCategory = catSlug,
                    IsActive = true,
                    IsPinned = false,
                    PublishedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90)),
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
