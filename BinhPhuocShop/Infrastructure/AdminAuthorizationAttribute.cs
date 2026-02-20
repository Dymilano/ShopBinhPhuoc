using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BinhPhuocShop.Infrastructure;

public class AdminAuthorizationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userRole = context.HttpContext.Session.GetString("UserRole");
        var userId = context.HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "Admin", returnUrl = context.HttpContext.Request.Path });
            return;
        }
        
        // Chỉ cho phép Admin và Manager truy cập
        if (userRole != "Admin" && userRole != "Manager")
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "Admin" });
            return;
        }
        
        base.OnActionExecuting(context);
    }
}
