// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Guflow.Properties;

namespace Guflow.Decider
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

        public WorkflowEventMethod EventMethod(EventName eventName)
        {
            var foundMethod = MethodFindingStrategy.MatchByEventName(eventName).Find(_allTargetMethods, eventName);
            if (foundMethod == null) return null;
            return new WorkflowEventMethod(_targetInstance,foundMethod);
        }

        public WorkflowEventMethod SignalEventMethod(string signalName)
        {
            var signalMethods = _allTargetMethods.Filter<SignalEventAttribute>();
            var strategy = MethodFindingStrategy.Composite(MethodFindingStrategy.MatchBySignalNameProperty(signalName), MethodFindingStrategy.MatchBySignalMethodName(signalName));
            var foundMethod = strategy.Find(signalMethods, EventName.Signal);
            if (foundMethod == null) return EventMethod(EventName.Signal);
            return new WorkflowEventMethod(_targetInstance, foundMethod);
        }

        

        internal interface IMethodFindingStrategy
        {
            MethodInfo Find(IEnumerable<MethodInfo> methods, EventName eventName);
        }

        internal class MethodFindingStrategy : IMethodFindingStrategy
        {
            private readonly Func<IEnumerable<MethodInfo>, IEnumerable<MethodInfo>> _filter;

            private MethodFindingStrategy(Func<IEnumerable<MethodInfo>, IEnumerable<MethodInfo>> filter)
            {
                _filter = filter;
            }

            public static IMethodFindingStrategy MatchBySignalMethodName(string signalName) => new MethodFindingStrategy(m=>m.Filter(signalName));
            public static IMethodFindingStrategy MatchBySignalNameProperty(string signalName) => new MethodFindingStrategy(m=> m.Filter<SignalEventAttribute>(s => s.IsFor(signalName)));
            public static IMethodFindingStrategy MatchByEventName(EventName eventName) => new MethodFindingStrategy(m=> m.Filter<WorkflowEventAttribute>(s => s.IsFor(eventName)));

            public static IMethodFindingStrategy Composite(params IMethodFindingStrategy[] strategies) => new ChainedMethodFindingStrategy(strategies);

            public MethodInfo Find(IEnumerable<MethodInfo> methods, EventName eventName)
            {
                var matchingMethods = _filter(methods).ToArray();
                if (!matchingMethods.Any())
                    return null;
                if (matchingMethods.Count() > 1)
                    throw new AmbiguousWorkflowMethodException(string.Format(Resources.Multiple_event_methods, eventName));
                var method=  matchingMethods.First();
                if (!HasValidReturnType(method))
                    throw new InvalidMethodSignatureException(string.Format(Resources.Invalid_return_type, method.Name, typeof(WorkflowAction), typeof(void)));
                return method;
            }

            private bool HasValidReturnType(MethodInfo targetMethod)
            {
                var workflowActionType = typeof(WorkflowAction);
                var targetMethodReturnType = targetMethod.ReturnType;
                return workflowActionType.IsAssignableFrom(targetMethodReturnType) ||
                       targetMethodReturnType == typeof(void);
            }

            private class ChainedMethodFindingStrategy : IMethodFindingStrategy
            {
                private readonly IMethodFindingStrategy[] _strategies;

                public ChainedMethodFindingStrategy(params IMethodFindingStrategy[] strategies)
                {
                    _strategies = strategies;
                }

                public MethodInfo Find(IEnumerable<MethodInfo> methods, EventName eventName)
                {
                    foreach (var strategy in _strategies)
                    {
                        var method = strategy.Find(methods, eventName);
                        if (method != null) return method;
                    }

                    return null;
                }
            }
        }
    }
}