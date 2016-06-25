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
    }
}