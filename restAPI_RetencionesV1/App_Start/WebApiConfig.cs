using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace restAPI_RetencionesV1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de API web

            // Rutas de API web
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{valor1}/{valor2}/{valor3}/{valor4}/{valor5}",
                defaults: new { valor1 = RouteParameter.Optional , valor2 = RouteParameter.Optional , valor3 = RouteParameter.Optional , valor4 = RouteParameter.Optional , valor5 = RouteParameter.Optional }
            );
        }
    }
}
