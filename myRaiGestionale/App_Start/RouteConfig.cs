using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace myRaiGestionale
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.png");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*assets}", new { assets = @"assets/.*" });


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Scrivania", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "myRaiGestionale.Controllers" }
            );

            routes.MapRoute(
                name: "FileDisplay",
                url: "{controller}/{action}/file/{title}",
                defaults: new { controller = "Home", action = "NotAuth", title = UrlParameter.Optional },
                namespaces: new string[] { "myRaiGestionale.Controllers" }
            );

        }
    }
}