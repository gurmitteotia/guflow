using System;
using System.Collections.Generic;
using System.Reflection;
using Guflow.Decider;
using Guflow.Properties;

namespace Guflow
{
    internal static class MethodInfoExtensions
    {
        public static object[] BuildParametersFrom(this MethodInfo method, object sourceArguments)
        {
            var parameters = new List<object>();
            var invokedArgumentType = sourceArguments.GetType();
            var properties = BuildPropertyDictionaryFrom(sourceArguments);
            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(invokedArgumentType))
                {
                    parameters.Add(sourceArguments);
                    continue;
                }
                object propertyValue;
                if (properties.TryGetValue(parameterInfo.Name, out propertyValue))
                {
                    if (propertyValue != null && parameterInfo.ParameterType.IsInstanceOfType(propertyValue))
                        parameters.Add(propertyValue);
                    else if(propertyValue.IsValidJson() && !parameterInfo.ParameterType.Primitive())
                        parameters.Add(((string)propertyValue).FromJson(parameterInfo.ParameterType));
                    else if (propertyValue == null)
                        parameters.Add(DefaultValueFor(parameterInfo.ParameterType));
                    else if (propertyValue.Primitive() && parameterInfo.ParameterType.IsString())
                        parameters.Add(propertyValue.ToString());
                    else
                        throw new InvalidMethodSignatureException(string.Format(Resources.Invalid_parameter, method.Name, parameterInfo.Name, invokedArgumentType.Name ));

                }
                else
                    parameters.Add(DefaultValueFor(parameterInfo.ParameterType));
            }

            return parameters.ToArray();
        }

        private static Dictionary<string, object> BuildPropertyDictionaryFrom(object argument)
        {
            var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var argumentType = argument.GetType();
            var sourceProperties = argumentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (var sourceProperty in sourceProperties)
            {
                var propertyValue = sourceProperty.GetValue(argument);
                properties.Add(sourceProperty.Name, propertyValue);
            }

            return properties;
        }

        private static object DefaultValueFor(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}