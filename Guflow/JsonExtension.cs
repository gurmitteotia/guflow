using Newtonsoft.Json;

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

        internal static string ToAwsString(this object instance)
        {
            if (instance == null)
                return null;
            var inputAsString = instance as string;
            if (inputAsString != null)
                return inputAsString;
            return instance.ToJson();
        }
    }
}