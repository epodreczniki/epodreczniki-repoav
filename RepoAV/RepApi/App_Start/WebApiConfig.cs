using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PSNC.RepoAV.Services.RepApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "MetadataApi",
               routeTemplate: "api/Element/{id}/metadata",
               defaults: new { controller = "Element", action = "metadata", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ElementApi",
                routeTemplate: "api/Element/{id}",
                defaults: new { controller = "Element", action = "get", id = RouteParameter.Optional }                
            );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManagerApi",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }


    }
}
