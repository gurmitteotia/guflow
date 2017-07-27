using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Guflow
{
    public static class SerializationExtension
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
            catch (JsonReaderException)
            {
                return false;
            }
        }

        internal static string ToAwsString(this object instance)
        {
            if (instance == null)
                return null;
            if (Primitive(instance))
                return instance.ToString();

            return instance.ToJson();
        }

        internal static bool Primitive(this object obj)
        {
            return Primitive(obj.GetType());
        }
        internal static bool Primitive(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime)|| type==typeof(TimeSpan);
        }
        internal static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
        internal static bool IsString(this object obj)
        {
            return IsString(obj.GetType());
        }
    }
}