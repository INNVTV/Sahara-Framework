using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Sahara.Api.Accounts.Registration
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Enable CORS (Requires Asp.Net Cross Origin Support Nuget Packages)
            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
