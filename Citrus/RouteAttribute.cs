using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Citrus
{
    public class RouteAttribute : Attribute
    {
        public string Route { get; private set; }

        public RouteAttribute(string route)
        {
            Route = route;
        }
    }
}