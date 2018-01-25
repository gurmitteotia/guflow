// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal interface IDescriptionStrategy
    {
        WorkflowDescription FindDescription(Type activityType);
    }

    internal class CompositeDescriptionStrategy : IDescriptionStrategy
    {
        private readonly IEnumerable<IDescriptionStrategy> _strategies;

        public CompositeDescriptionStrategy(IEnumerable<IDescriptionStrategy> strategies)
        {
            _strategies = strategies;
        }

        public WorkflowDescription FindDescription(Type activityType)
        {
            foreach (var strategy in _strategies)
            {
                var description = strategy.FindDescription(activityType);
                if (description != null) return description;
            }
            return null;
        }
    }

    internal class DescriptionStrategy : IDescriptionStrategy
    {
        public static readonly DescriptionStrategy FromAttribute = new DescriptionStrategy(
            t => t.GetCustomAttribute<WorkflowDescriptionAttribute>()?.WorkflowDescription());
        public static readonly DescriptionStrategy FactoryMethod = new DescriptionStrategy(BuildFromFactoryMethod);

        private readonly Func<Type, WorkflowDescription> _strategyFunc;

        private DescriptionStrategy(Func<Type, WorkflowDescription> strategyFunc)
        {
            _strategyFunc = strategyFunc;
        }

        public WorkflowDescription FindDescription(Type activityType)
        {
            return _strategyFunc(activityType);
        }

        private static WorkflowDescription BuildFromFactoryMethod(Type workflowType)
        {

            var method = workflowType.GetMethods(BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic| BindingFlags.Public)
                .FirstOrDefault(IsFactoryMethod);
            return (WorkflowDescription)method?.Invoke(null, null);
        }

        private static bool IsFactoryMethod(MethodInfo method)
            => method.ReturnType == typeof(WorkflowDescription) && method.GetParameters().Length == 0;

    }
}