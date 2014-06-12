using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Citrus
{
    public class Method
    {
        protected Func<object> Func { get; set; }

        public static implicit operator Method(Func<object> func)
        {
            return new Method { Func = func };
        }

        public static implicit operator Func<object>(Method method)
        {
            return method.Func;
        }
    }

    public delegate object SimpleFunc();
}
