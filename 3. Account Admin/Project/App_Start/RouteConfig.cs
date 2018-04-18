using System.Web.Mvc;
using System.Web.Routing;

namespace AccountAdminSite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Inventory", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
