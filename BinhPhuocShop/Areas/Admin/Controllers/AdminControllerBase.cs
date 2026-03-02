using BinhPhuocShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinhPhuocShop.Areas.Admin.Controllers;

public class AdminControllerBase : Controller
{
    protected readonly AppDbContext Db;

    public AdminControllerBase(AppDbContext db)
    {
        Db = db;
    }

    public override async Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
    {
        // Populate ViewBag for header notifications
        ViewBag.PendingOrdersCount = await Db.Orders.CountAsync(o => o.Status == "pending");
        ViewBag.UnreadContactsCount = await Db.ContactMessages.CountAsync(m => !m.IsRead);
        
        await next();
    }
}
