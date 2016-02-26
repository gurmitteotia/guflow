using System.Runtime.Serialization;

namespace Guflow
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