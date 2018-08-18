using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

namespace WebApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "SoundApi",
                routeTemplate: "api/{controller}/{action}/{locationId}/{trackId}",
                defaults: new { controller = "sound",
                                action = "play",
                                locationId = UrlParameter.Optional,
                                trackId = UrlParameter.Optional }
            );
        }
    }
}
