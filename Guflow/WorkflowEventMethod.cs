using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Guflow.Properties;

namespace Guflow
{
    internal class WorkflowEventMethod
    {
        private readonly MethodInfo _methodInfo;
        private readonly object _targetInstance;

        public WorkflowEventMethod(object targetInstance, MethodInfo methodInfo)
        {
            _targetInstance = targetInstance;
            _methodInfo = methodInfo;
        }
        public WorkflowAction Invoke(WorkflowEvent argument)
        {
            var parameters = BuildParametersFrom(argument);

            try
            {
                var returnType = _methodInfo.Invoke(_targetInstance, parameters);
                return returnType == null ? WorkflowAction.Ignore : (WorkflowAction)returnType;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        private object[] BuildParametersFrom(WorkflowEvent argument)
        {
            var parameters = new List<object>();
            var invokedArgumentType = argument.GetType();
            var properties = BuildPropertyDictionaryFrom(argument);
            foreach (var parameterInfo in _methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(invokedArgumentType))
                {
                    parameters.Add(argument);
                    continue;
                }
                object propertyValue;
                if (properties.TryGetValue(parameterInfo.Name, out propertyValue))
                {
                    if (propertyValue != null && parameterInfo.ParameterType.IsInstanceOfType(propertyValue))
                        parameters.Add(propertyValue);
                    else if (propertyValue == null && !parameterInfo.ParameterType.IsValueType)
                        parameters.Add(propertyValue);
                    else
                        throw new InvalidMethodSignatureException(string.Format(Resources.Invalid_parameter, _methodInfo.Name, parameterInfo.Name, invokedArgumentType.Name));
 
                }
                else
                    parameters.Add(DefaultValueFor(parameterInfo.ParameterType));
            }

            return parameters.ToArray();
        }

        private Dictionary<string, object> BuildPropertyDictionaryFrom(WorkflowEvent argument)
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

        private object DefaultValueFor(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}