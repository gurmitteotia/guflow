using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Guflow.Decider
{
    internal static class IdentityExtensions
    {
        public static string ToJson(this Identity identity)
        {
            var jsonObject = new JsonFormat()
            {
                Name = identity.Name,
                Ver = identity.Version,
                PName = identity.PositionalName
            };

            return jsonObject.ToJson();
        }
        public static Identity FromJson(this string jsonIdenity)
        {
            var jsonObject = jsonIdenity.FromJson<JsonFormat>();
            return Identity.New(jsonObject.Name,jsonObject.Ver,jsonObject.PName);
        }

        public static string GetMd5Hash(this string data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
                var hashBuffer = new StringBuilder();
                foreach (var hashByte in hash)
                {
                    hashBuffer.Append(hashByte.ToString("X2"));
                }
                return hashBuffer.ToString();
            }
        }

        [DataContract]
        private class JsonFormat
        {
            [DataMember]
            public string Name;
            [DataMember]
            public string Ver;
            [DataMember]
            public string PName;
        }
    }
}