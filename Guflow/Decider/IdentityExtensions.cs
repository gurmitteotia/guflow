using System;
using System.Security.Cryptography;
using System.Text;

namespace Guflow.Decider
{
    internal class IdentityFormat
    {
        private readonly Func<Identity, string> _serializeFunc;
        private readonly Func<string, Identity> _deserializeFunc;

        public static IdentityFormat Json = new IdentityFormat(ToJson, FromJson);

        private IdentityFormat(Func<Identity, string> serializeFunc, Func<string, Identity> deserializeFunc)
        {
            _serializeFunc = serializeFunc;
            _deserializeFunc = deserializeFunc;
        }

        private static Identity FromJson(string data)
        {
            var jsonObject = data.FromJson<JsonFormat>();
            return Identity.New(jsonObject.Name, jsonObject.Ver, jsonObject.PName);
        }

        private static string ToJson(Identity identity)
        {
            var jsonObject = new JsonFormat()
            {
                Name = identity.Name,
                Ver = identity.Version,
                PName = identity.PositionalName
            };

            return jsonObject.ToJson();
        }

        public string Serialize(Identity identity)
        {
            return _serializeFunc(identity);
        }

        public Identity Deserialize(string data)
        {
            return _deserializeFunc(data);
        }
        internal class JsonFormat
        {
            public string Name;
            public string Ver;
            public string PName;
        }

    }
    internal static class IdentityExtensions
    {
      
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

        
    }
}