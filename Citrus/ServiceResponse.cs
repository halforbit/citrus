using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Citrus
{
    public class ServiceResponse
    {
        public bool Success { get; set; }

        public object Response { get; set; }
    }
}