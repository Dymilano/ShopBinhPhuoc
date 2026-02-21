using BinhPhuocShop.Data;
using BinhPhuocShop.Models;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Services;

public class ActivityLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContext;

    public ActivityLogService(AppDbContext db, IHttpContextAccessor httpContext)
    {
        _db = db;
        _httpContext = httpContext;
    }

    public async Task LogAsync(string action, string entityType, int? entityId = null, string? entityName = null, string? details = null)
    {
        var ctx = _httpContext.HttpContext;
        var userId = ctx?.Session.GetString("UserId");
        var userEmail = ctx?.Session.GetString("UserEmail");
        var userName = ctx?.Session.GetString("UserName");
        var role = ctx?.Session.GetString("UserRole") ?? "Customer";
        var ip = ctx?.Connection?.RemoteIpAddress?.ToString();

        int? uid = int.TryParse(userId, out var id) ? id : null;

        var log = new ActivityLog
        {
            UserId = uid,
            UserEmail = userEmail,
            UserName = userName ?? "Guest",
            Role = role,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            Details = details,
            IpAddress = ip
        };
        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
