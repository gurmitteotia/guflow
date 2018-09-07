// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
#if NETCOREAPP2_1
using Microsoft.Extensions.Configuration;
#endif
namespace Guflow.IntegrationTests
{
    public class Configuration
    {
        private readonly Func<string, string> _valueName;
        private Configuration(Func<string, string> valueName)
        {
            _valueName = valueName;
        }

        public static Configuration Build()
        {
#if NET46
                return new Configuration(n=>System.Configuration.ConfigurationManager.AppSettings[n]);
#elif NETCOREAPP2_1
            var configurationBuilder = new ConfigurationBuilder();
           
            var builder = configurationBuilder.AddJsonFile("appSettings.json", true)
                .SetBasePath(System.AppContext.BaseDirectory)
                .Build();
            
            return new Configuration(n => builder[n]);

#endif

        }

        public string this[string name] => _valueName(name);
    }
}