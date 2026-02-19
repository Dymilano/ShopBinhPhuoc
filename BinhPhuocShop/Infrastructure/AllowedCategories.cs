namespace BinhPhuocShop.Infrastructure;

/// <summary>
/// Chỉ 5 danh mục chính: Giày nam, Giày nữ, Dép nam, Dép nữ, Phụ kiện.
/// Dùng cho seed, cleanup, menu web và admin.
/// </summary>
public static class AllowedCategories
{
    public static readonly string[] Slugs = { "giay-nam", "giay-nu", "dep-nam", "dep-nu", "phu-kien" };

    public static readonly (string Name, string Slug, int DisplayOrder)[] Defaults = new[]
    {
        ("Giày nam", "giay-nam", 1),
        ("Giày nữ", "giay-nu", 2),
        ("Dép nam", "dep-nam", 3),
        ("Dép nữ", "dep-nu", 4),
        ("Phụ kiện", "phu-kien", 5),
    };

    public static bool IsAllowed(string? slug) =>
        !string.IsNullOrEmpty(slug) && Slugs.Contains(slug, StringComparer.OrdinalIgnoreCase);
}
