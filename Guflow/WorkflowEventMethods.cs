using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.Auth.AccessControlPolicy;
using Guflow.Properties;

namespace Guflow
{
    internal class WorkflowEventMethods
    {
        private readonly object _targetInstance;
        private readonly IEnumerable<MethodInfo> _allTargetMethods; 
        private WorkflowEventMethods(object targetInstance)
        {
            _targetInstance = targetInstance;
            _allTargetMethods = targetInstance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic|BindingFlags.Public);
        }

        public static WorkflowEventMethods For(object targetInstance)
        {
            return new WorkflowEventMethods(targetInstance);
        }

        public WorkflowEventMethod FindFor<TAttribute>() where TAttribute:Attribute
        {
            var matchingMethods =_allTargetMethods.Where(m => m.GetCustomAttributes<TAttribute>().Any());
            if (!matchingMethods.Any())
                return null;
            if(matchingMethods.Count()>1)
                throw new AmbiguousWorkflowMethodException(string.Format(Resources.Multiple_event_methods, typeof(TAttribute).Name));
            var foundMethod = matchingMethods.First();

            if(!HasValidReturnType(foundMethod))
                throw new InvalidMethodSignatureException(string.Format(Resources.Invalid_return_type,foundMethod.Name,typeof(WorkflowAction), typeof(void)));
            return new WorkflowEventMethod(_targetInstance,foundMethod);
        }

        private bool HasValidReturnType(MethodInfo targetMethod)
        {
            var workflowActionType = typeof (WorkflowAction);
            var targetMethodReturnType = targetMethod.ReturnType;
            return workflowActionType.IsAssignableFrom(targetMethodReturnType) ||
                   targetMethodReturnType == typeof (void);
        }
    }
}