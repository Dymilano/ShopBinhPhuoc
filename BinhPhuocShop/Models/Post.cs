namespace BinhPhuocShop.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public string? BlogCategory { get; set; } // phong-cach, tin-tuc-moi-ve-mulgati, bao-chi, goc-review
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
