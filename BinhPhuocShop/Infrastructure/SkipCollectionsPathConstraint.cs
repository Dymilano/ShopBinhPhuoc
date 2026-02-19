using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BinhPhuocShop.Infrastructure;

/// <summary>
/// Ngăn route mặc định match URL bắt đầu bằng /collections/ để tránh AmbiguousMatchException
/// (route "collections/{slug}" và "{controller}/{action}/{id?}" đều match /collections/xxx).
/// </summary>
public class SkipCollectionsPathConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (routeDirection != RouteDirection.IncomingRequest)
            return true;
        var path = httpContext?.Request.Path.Value ?? "";
        if (path.StartsWith("/collections/", StringComparison.OrdinalIgnoreCase) || path.Equals("/collections", StringComparison.OrdinalIgnoreCase))
            return false;
        if (routeKey == "controller" && values.TryGetValue("controller", out var c) && string.Equals(c?.ToString(), "Collections", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
