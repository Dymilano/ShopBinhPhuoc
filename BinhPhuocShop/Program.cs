using BinhPhuocShop.Data;
using BinhPhuocShop.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromDays(7); options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BinhPhuocShop.Services.CartService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Ensure Order tables exist (for existing databases created before Order was added)
    try { db.Database.ExecuteSqlRaw("SELECT 1 FROM Orders LIMIT 1"); }
    catch
    {
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS Orders (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            OrderCode TEXT NOT NULL,
            CustomerName TEXT NOT NULL,
            Phone TEXT NOT NULL,
            Email TEXT NOT NULL,
            Address TEXT NOT NULL,
            Note TEXT,
            Status TEXT NOT NULL DEFAULT 'pending',
            TotalAmount REAL NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT
        )");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS OrderItems (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            OrderId INTEGER NOT NULL,
            ProductId INTEGER NOT NULL,
            ProductName TEXT NOT NULL,
            ProductImageUrl TEXT,
            Size TEXT,
            Price REAL NOT NULL,
            Quantity INTEGER NOT NULL,
            FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
            FOREIGN KEY (ProductId) REFERENCES Products(Id)
        )");
    }
    try { db.Database.ExecuteSqlRaw("SELECT 1 FROM Users LIMIT 1"); }
    catch
    {
        db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Email TEXT NOT NULL,
            PasswordHash TEXT NOT NULL,
            Name TEXT NOT NULL,
            Phone TEXT,
            Address TEXT,
            Role TEXT NOT NULL DEFAULT 'Customer',
            IsActive INTEGER NOT NULL DEFAULT 1,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT
        )");
    }
    // Add missing columns to Users table if they don't exist
    try
    {
        db.Database.ExecuteSqlRaw("SELECT Address FROM Users LIMIT 1");
    }
    catch
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN Address TEXT");
    }
    try
    {
        db.Database.ExecuteSqlRaw("SELECT Role FROM Users LIMIT 1");
    }
    catch
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN Role TEXT NOT NULL DEFAULT 'Customer'");
    }
    try
    {
        db.Database.ExecuteSqlRaw("SELECT UpdatedAt FROM Users LIMIT 1");
    }
    catch
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN UpdatedAt TEXT");
    }
    try
    {
        db.Database.ExecuteSqlRaw("SELECT BlogCategory FROM Posts LIMIT 1");
    }
    catch
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE Posts ADD COLUMN BlogCategory TEXT");
    }
    if (!db.SiteSettings.Any())
    {
        db.SiteSettings.AddRange(
            new BinhPhuocShop.Models.SiteSetting { Key = "SiteName", Value = "Bình Phước Shop" },
            new BinhPhuocShop.Models.SiteSetting { Key = "SiteDescription", Value = "Thương hiệu chuyên kinh doanh, phân phối giày da nam. Chất lượng cao, nhiều bộ sưu tập độc đáo." },
            new BinhPhuocShop.Models.SiteSetting { Key = "Phone", Value = "0984843218" },
            new BinhPhuocShop.Models.SiteSetting { Key = "Email", Value = "contact@binhphuocshop.vn" },
            new BinhPhuocShop.Models.SiteSetting { Key = "Address", Value = "123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh" },
            new BinhPhuocShop.Models.SiteSetting { Key = "WebsiteUrl", Value = "https://binhphuocshop.vn" });
        db.SaveChanges();
    }
    else
    {
        // Đảm bảo WebsiteUrl luôn có
        if (!db.SiteSettings.Any(s => s.Key == "WebsiteUrl"))
        {
            db.SiteSettings.Add(new BinhPhuocShop.Models.SiteSetting { Key = "WebsiteUrl", Value = "https://binhphuocshop.vn" });
            db.SaveChanges();
        }
    }
    // Seed admin user - đảm bảo luôn có Role = Admin và Address
    var adminEmail = "admin@binhphuocshop.vn";
    var adminUser = db.Users.FirstOrDefault(u => u.Email == adminEmail);
    var adminHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("admin123"));
    var adminHashBase64 = Convert.ToBase64String(adminHash);
    
    if (adminUser == null)
    {
        adminUser = new BinhPhuocShop.Models.User
        {
            Email = adminEmail,
            PasswordHash = adminHashBase64,
            Name = "Administrator",
            Role = "Admin",
            Address = "123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(adminUser);
    }
    else
    {
        // Cập nhật Role, Address và password nếu cần
        adminUser.Role = "Admin";
        adminUser.Address = adminUser.Address ?? "123 Đường ABC, Quận XYZ, TP. Hồ Chí Minh";
        adminUser.PasswordHash = adminHashBase64; // Đảm bảo password luôn đúng
        adminUser.IsActive = true;
        adminUser.UpdatedAt = DateTime.UtcNow;
    }
    db.SaveChanges();
    var allowedSlugs = BinhPhuocShop.Infrastructure.AllowedCategories.Slugs;
    var requiredCategories = BinhPhuocShop.Infrastructure.AllowedCategories.Defaults;

    if (!db.Categories.Any())
    {
        foreach (var (name, slug, order) in requiredCategories)
            db.Categories.Add(new BinhPhuocShop.Models.Category { Name = name, Slug = slug, DisplayOrder = order, IsActive = true });
        db.SaveChanges();
    }
    else
    {
        foreach (var (name, slug, order) in requiredCategories)
        {
            if (!db.Categories.Any(c => c.Slug == slug))
            {
                db.Categories.Add(new BinhPhuocShop.Models.Category { Name = name, Slug = slug, DisplayOrder = order, IsActive = true });
                db.SaveChanges();
            }
        }

        var firstAllowedId = await db.Categories.Where(c => allowedSlugs.Contains(c.Slug ?? "")).OrderBy(c => c.Id).Select(c => c.Id).FirstOrDefaultAsync();
        if (firstAllowedId == 0 && await db.Categories.AnyAsync())
            firstAllowedId = (await db.Categories.OrderBy(c => c.Id).FirstAsync()).Id;

        var toRemove = await db.Categories.Where(c => c.Slug == null || !allowedSlugs.Contains(c.Slug)).ToListAsync();
        foreach (var cat in toRemove)
        {
            var productsToReassign = await db.Products.Where(p => p.CategoryId == cat.Id).ToListAsync();
            foreach (var p in productsToReassign) p.CategoryId = firstAllowedId;
            db.Categories.Remove(cat);
        }
        await db.SaveChangesAsync();

        foreach (var slug in allowedSlugs)
        {
            var withSlug = await db.Categories.Where(c => c.Slug == slug).OrderBy(c => c.Id).ToListAsync();
            if (withSlug.Count <= 1) continue;
            var keep = withSlug[0];
            foreach (var dup in withSlug.Skip(1))
            {
                var productsToReassign = await db.Products.Where(p => p.CategoryId == dup.Id).ToListAsync();
                foreach (var p in productsToReassign) p.CategoryId = keep.Id;
                db.Categories.Remove(dup);
            }
        }
        await db.SaveChangesAsync();
    }
    await MulgatiSeed.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();

var cozaPath = Path.Combine(builder.Environment.ContentRootPath, "..", "cozastore-master", "cozastore-master");
if (Directory.Exists(cozaPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(cozaPath),
        RequestPath = "/store"
    });
}
var cleopatraPath = Path.Combine(builder.Environment.ContentRootPath, "..", "cleopatra-tailwind-1.0.0", "cleopatra-tailwind-1.0.0", "dist");
if (Directory.Exists(cleopatraPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(cleopatraPath),
        RequestPath = "/admin"
    });
}
// Serve Duralux admin assets
var duraluxAssetsPath = Path.Combine(builder.Environment.WebRootPath, "duralux");
if (Directory.Exists(duraluxAssetsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(duraluxAssetsPath),
        RequestPath = "/duralux"
    });
}

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "gioi-thieu",
    pattern: "gioi-thieu",
    defaults: new { controller = "Pages", action = "GioiThieu" });

app.MapControllerRoute(
    name: "pages-direct",
    pattern: "Pages/{action=Index}",
    defaults: new { controller = "Pages" });

app.MapControllerRoute(
    name: "pages",
    pattern: "pages/{slug}",
    defaults: new { controller = "Pages", action = "Show" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    constraints: new { controller = new SkipCollectionsPathConstraint() });

app.Run();
