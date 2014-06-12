using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace Citrus
{
    public static class CitrusExtensions
    {
        static CitrusExtensions()
        {
            SerializeJsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            DeserializeJsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new PascalCasePropertyNamesContractResolver()
            };
        }

        class PascalCasePropertyNamesContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
            }
        }

        static JsonSerializerSettings SerializeJsonSerializerSettings { get; set; }

        static JsonSerializerSettings DeserializeJsonSerializerSettings { get; set; }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(
                obj,
                Formatting.Indented,
                SerializeJsonSerializerSettings);
        }

        public static dynamic JsonToDynamic(this string json)
        {
            return JsonConvert.DeserializeObject(
                json,
                DeserializeJsonSerializerSettings);
        }

        public static IEnumerable<Type> GetSubclasses(this Type type)
        {
            return type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type));
        }

        public static string PascalCaseToLowerCaseDashed(this string properCaseString)
        {
            return Regex
                .Replace(
                    properCaseString,
                    @"(?<=[a-z])[A-Z]",
                    match => "-" + match.Value.ToLower())
                .ToLower();
        }
    }
}