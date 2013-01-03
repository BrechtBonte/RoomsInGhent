using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RoomsInGhent {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{par1}/{par2}/{par3}",
                defaults: new { par1 = RouteParameter.Optional, par2 = RouteParameter.Optional, par3 = RouteParameter.Optional }
            );
        }
    }
}
