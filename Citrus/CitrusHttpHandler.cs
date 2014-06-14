
namespace Citrus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using Newtonsoft.Json;

    public class CitrusHttpHandler : IHttpHandler
    {
        #region Static Members

        static Dictionary<string, string> RoutePatterns { get; set; }

        static Type[] ControllerTypes { get; set; }

        public static void RegisterControllerAssembly(Type typeInAssembly)
        {
            var assembly = typeInAssembly.Assembly;

            ControllerTypes = assembly
                .GetTypes()
                .Where(type => type.GetCustomAttribute<ControllerAttribute>() != null)
                .ToArray();

            RoutePatterns = BuildRoutePatterns(ControllerTypes);
        }

        static bool TryServeRoutes(HttpContext context)
        {
            if (context.Request.Url.LocalPath == "/routes.js")
            {
                context.Response.Write("var Routes = " + RoutePatterns.ToJson() + ";");

                context.Response.ContentType = GetContentTypeForExtension(".js");

                context.Response.End();

                return true;
            }

            return false;
        }

        static bool TryServeIndex(HttpContext httpContext)
        {
            if (httpContext.Request.Url.LocalPath == "/")
            {
                httpContext.Response.ContentType = "text/html";

                httpContext.Response.ContentEncoding = Encoding.UTF8;

                httpContext.Response.WriteFile("Content/index.html");

                httpContext.Response.End();

                return true;
            }

            return false;
        }

        static bool TryServeService(HttpContext context)
        {
            var path = context.Request.Url.LocalPath.ToLower();

            if (path.StartsWith("/service/"))
            {
                var route = path.Substring("/service/".Length);

                var routeSegments = route.Split(
                    new char[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var controllerType in ControllerTypes)
                {
                    var routeValues = new List<string>();

                    var method = ResolveMethodForRoute(
                        controllerType,
                        routeSegments,
                        context.Request.HttpMethod.ToLower(),
                        routeValues);

                    if (method != null)
                    {
                        var instance = Activator.CreateInstance(method.DeclaringType);

                        var arguments = BuildArgumentsForControllerMethodCall(
                            context,
                            method,
                            routeValues);

                        var result = method.Invoke(instance, arguments);

                        var json = new ServiceResponse
                        {
                            Success = true,

                            Response = result
                        }
                        .ToJson();

                        context.Response.ContentType = "application/json";

                        context.Response.Write(json);

                        context.Response.End();

                        return true;
                    }
                }
            }

            return false;
        }

        static object[] BuildArgumentsForControllerMethodCall(
            HttpContext context,
            MethodInfo method,
            List<string> routeValues)
        {
            var values = new Queue<string>(routeValues);

            var arguments = new List<object>();

            var parameters = method.GetParameters();

            if (!parameters.Any()) return new object[0];

            foreach (var parameter in parameters)
            {
                if (parameter.Name == "body")
                {
                    var requestStream = context.Request.GetBufferlessInputStream();

                    var bodyText = new StreamReader(requestStream).ReadToEnd();

                    arguments.Add(bodyText.JsonToDynamic());
                }
                else
                {
                    var value = values.Dequeue();

                    arguments.Add(Convert.ChangeType(value, parameter.ParameterType));
                }
            }

            return arguments.ToArray();
        }

        static MethodInfo ResolveMethodForRoute(
            Type controllerType,
            string[] routeSegments,
            string method,
            List<string> routeValues)
        {
            if (routeSegments.Length == 0)
            {
                return controllerType
                    .GetMethods()
                    .FirstOrDefault(m => m.Name.PascalCaseToLowerCaseDashed() == method);
            }
            else
            {
                var nestedTypes = controllerType.GetNestedTypes();

                var nestedControllerType = nestedTypes
                    .FirstOrDefault(t => t.Name.PascalCaseToLowerCaseDashed() == routeSegments[0]);

                if (nestedControllerType != null)
                {
                    return ResolveMethodForRoute(
                        nestedControllerType,
                        routeSegments.Skip(1).ToArray(),
                        method,
                        routeValues);
                }
                else
                {
                    nestedControllerType = nestedTypes.FirstOrDefault(t => t.Name.StartsWith("_"));

                    if (nestedControllerType != null)
                    {
                        routeValues.Add(routeSegments[0]);

                        return ResolveMethodForRoute(
                            nestedControllerType,
                            routeSegments.Skip(1).ToArray(),
                            method,
                            routeValues);
                    }
                }
            }

            return null;
        }

        static bool TryServeContent(HttpContext context)
        {
            var path = context.Request.Url.LocalPath;

            var contentType = GetContentTypeForExtension(path);

            if (contentType != null)
            {
                var filename = context.Server.MapPath("~/Content" + path);

                if (File.Exists(filename))
                {
                    context.Response.ContentType = contentType;

                    context.Response.WriteFile(filename);

                    context.Response.End();

                    return true;
                }
            }

            return false;
        }

        static void ServeNotFound(HttpContext context)
        {
            context.Response.StatusCode = 404;

            context.Response.End();
        }

        static string GetContentTypeForExtension(string filename)
        {
            var extension = Path.GetExtension(filename).ToLower();

            switch (extension)
            {
                case ".css": return "text/css";

                case ".html": return "text/html";

                case ".ico": return "image/x-icon";

                case ".js": return "application/x-javascript";

                default: return null;
            }
        }

        static Dictionary<string, string> BuildRoutePatterns(Type[] controllerTypes)
        {
            var routePatterns = new Dictionary<string, string>();

            foreach(var controllerType in controllerTypes)
            {
                BuildRoutePatternsForControllerType(
                    "", 
                    controllerType, 
                    controllerType, 
                    routePatterns);
            }

            return routePatterns;
        }

        static void BuildRoutePatternsForControllerType(
            string routePrefix, 
            Type rootType, 
            Type type, 
            Dictionary<string, string> routePatterns)
        {
            var segmentName = type != rootType ? type.Name : "";

            if (type.GetMethod("Get") != null)
            {
                var route = 
                    ((routePrefix != "" ? routePrefix + "/" : "") + segmentName)
                    .PascalCaseToLowerCaseDashed();

                var routeRegex = ReplacePlaceholdersWithRegexPatterns(route);

                routePatterns.Add(routeRegex, route);
            }

            foreach(var nestedType in type.GetNestedTypes())
            {
                BuildRoutePatternsForControllerType(
                    (routePrefix != "" ? routePrefix + "/" : "") + segmentName,
                    rootType,
                    nestedType,
                    routePatterns);
            }
        }

        static string ReplacePlaceholdersWithRegexPatterns(string route)
        {
            return "^" + Regex.Replace(
                route,
                @"_[^/$]*",
                match => "([^/]*)") + "$";
        }

        #endregion Static Members

        #region Non-Static Members

        public void ProcessRequest(HttpContext httpContext)
        {
            if (TryServeIndex(httpContext)) return;

            if (TryServeRoutes(httpContext)) return;

            if (TryServeService(httpContext)) return;

            if (TryServeContent(httpContext)) return;

            ServeNotFound(httpContext);
        }

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion Non-Static Members
    }
}