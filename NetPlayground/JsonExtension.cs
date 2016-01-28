using System.IO;
using System.Runtime.Serialization.Json;

namespace NetPlayground
{
    public static class JsonExtension
    {
        public static string ToJson<T>(this T instance)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, instance);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static T FromJson<T>(this string jsonData)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData)))
                return (T)serializer.ReadObject(memoryStream);
        }
    }
}