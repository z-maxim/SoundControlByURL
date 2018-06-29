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
                routeTemplate: "api/{controller}/{action}/{id}/{trackid}",
                defaults: new { controller = "sound",
                                action = "play",
                                id = UrlParameter.Optional,
                                trackid = UrlParameter.Optional }
            );
        }
    }
}
