// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Guflow
{
    public static class SerializationExtension
    {
        /// <summary>
        /// Serialize the instance in to JSON format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T instance)
        {
            return JsonConvert.SerializeObject(instance);
        }

        /// <summary>
        /// Deserialize a instance of type T from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static T As<T>(this string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        internal static object FromJson(this string jsonData, Type targetType)
        {
            return JsonConvert.DeserializeObject(jsonData, targetType);
        }

        /// <summary>
        /// Deserialize a JSON string in to dynamic object.
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static dynamic AsDynamic(this string jsonData)
        {
            if (jsonData.TryToParse(out JObject result))
                return result;
            return jsonData;
        }
        internal static bool IsValidJson(this object value)
        {
            var strValue = value as string;
            if (string.IsNullOrWhiteSpace(strValue))
                return false;
            strValue = strValue.Trim();
            if ((strValue.StartsWith("{") && strValue.EndsWith("}")) || //For object
                (strValue.StartsWith("[") && strValue.EndsWith("]"))) //For array
            {
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

            return false;
        }

        private static bool TryToParse(this object value, out JObject result)
        {
            result = null;
            var strValue = value as string;
            if (string.IsNullOrWhiteSpace(strValue))
                return false;
            try
            {
                result = JObject.Parse(strValue);
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

        internal static string ToLambdaInput(this object instance)
        {
            if (instance == null)
                return null;
            if (instance is string strInput)
                return strInput.IsValidJson()? strInput: EnsureEnclosedInQuotes(strInput);
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
            return type.GetTypeInfo().IsPrimitive || type == typeof(string) || type == typeof(DateTime)|| type==typeof(TimeSpan);
        }
        internal static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
        internal static bool IsString(this object obj)
        {
            return IsString(obj.GetType());
        }

        private static string EnsureEnclosedInQuotes(string str)
        {
            var trimmedStr = str.Trim('"');
            return $"\"{trimmedStr}\"";
        }
    }
}