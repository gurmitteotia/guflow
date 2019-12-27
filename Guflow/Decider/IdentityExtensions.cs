// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Security.Cryptography;
using System.Text;

namespace Guflow.Decider
{
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