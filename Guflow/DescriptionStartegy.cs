// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Guflow.Worker;

namespace Guflow
{
    internal interface IDescriptionStrategy
    {
        ActivityDescription FindDescription(Type activityType);
    }

    internal class CompositeDescriptionStrategy : IDescriptionStrategy
    {
        private readonly IEnumerable<IDescriptionStrategy> _strategies;

        public CompositeDescriptionStrategy(IEnumerable<IDescriptionStrategy> strategies)
        {
            _strategies = strategies;
        }

        public ActivityDescription FindDescription(Type activityType)
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
        public static DescriptionStrategy FromAttribute = new DescriptionStrategy(
                    t=>t.GetCustomAttribute<ActivityDescriptionAttribute>()?.ActivityDescription());
        public static DescriptionStrategy FactoryMethod = new DescriptionStrategy(BuildFromFactoryMethod);

        private readonly Func<Type, ActivityDescription> _strategyFunc;

        private DescriptionStrategy(Func<Type, ActivityDescription> strategyFunc)
        {
            _strategyFunc = strategyFunc;
        }

        public ActivityDescription FindDescription(Type activityType)
        {
            return _strategyFunc(activityType);
        }

        private static ActivityDescription BuildFromFactoryMethod(Type activityType)
        {
            
            var method = activityType.GetMethods(BindingFlags.Static | BindingFlags.GetField|BindingFlags.NonPublic)
                            .FirstOrDefault(IsFactoryMethod);
            return (ActivityDescription)method?.Invoke(null, null);
        }

        private static bool IsFactoryMethod(MethodInfo method)
            => method.ReturnType == typeof(ActivityDescription) && method.GetParameters().Length == 0;

    }
}