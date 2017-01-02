using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Guflow
{
    public static class JsonExtension
    {
        public static string ToJson<T>(this T instance)
        {
            return JsonConvert.SerializeObject(instance);
        }

        public static T FromJson<T>(this string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        internal static object FromJson(this string jsonData, Type targetType)
        {
            return JsonConvert.DeserializeObject(jsonData, targetType);
        }

        internal static bool IsValidJson(this object value)
        {
            var strValue = value as string;
            if (string.IsNullOrWhiteSpace(strValue))
                return false;
            try
            {
                JToken.Parse(strValue);
                return true;
            }
            catch (JsonReaderException exception)
            {
                return false;
            }
        }

        internal static string ToAwsString(this object instance)
        {
            if (instance == null)
                return null;
            var inputAsString = instance as string;
            if (inputAsString != null)
                return inputAsString;
            return instance.ToJson();
        }

        internal static bool Primitive(this object obj)
        {
            return obj.GetType().IsPrimitive || obj is string|| obj is DateTime;
        }
        internal static bool Primitive(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime);
        }
        internal static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
    }
}