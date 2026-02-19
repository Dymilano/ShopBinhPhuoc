using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BinhPhuocShop.Infrastructure;

/// <summary>
/// Ngăn route mặc định match khi controller = Collections để tránh AmbiguousMatchException
/// (route "collections/{slug}" và "{controller=Home}/{action=Index}/{id?}" đều match /collections/xxx).
/// </summary>
public class SkipCollectionsRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (routeDirection != RouteDirection.IncomingRequest)
            return true;
        if (routeKey != "controller")
            return true;
        if (values.TryGetValue("controller", out var controllerValue) && controllerValue != null)
        {
            var name = controllerValue.ToString();
            if (string.Equals(name, "Collections", StringComparison.OrdinalIgnoreCase))
                return false;
        }
        return true;
    }
}
