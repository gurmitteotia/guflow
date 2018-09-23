// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    internal class ActivityExecutionMethod
    {
        private readonly MethodInfo _executeMethod;
        public ActivityExecutionMethod(Type activityType)
        {
             _executeMethod = FindExecutionMethod(activityType);
        }
        private static MethodInfo FindExecutionMethod(Type activityType)
        {
            var allMethods = activityType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var executionMethods = allMethods.Where(m => m.GetCustomAttributes<ActivityMethodAttribute>().Any()).ToArray();
            if (!executionMethods.Any())
                throw new ActivityExecutionMethodException(string.Format(Resources.Activity_execution_method_missing, activityType.Name));
            if (executionMethods.Length > 1)
                throw new ActivityExecutionMethodException(string.Format(Resources.Multiple_activity_execution_methods_defined, activityType.Name));

            return executionMethods.First();
        }

        public async Task<ActivityResponse> ExecuteAsync(Activity activity, ActivityArgs activityArgs, CancellationToken cancellationToken)
        {
            var executionStrategy = ExecutionStrategy.CreateFor(_executeMethod);
            var parameters = _executeMethod.BuildParametersFrom(activityArgs, cancellationToken);
            return await executionStrategy.Execute(this, activity, parameters, activityArgs.TaskToken);
        }

        private object Execute(object targetInstance, object []parameters)
        {
            try
            {
                return _executeMethod.Invoke(targetInstance, parameters);
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                    throw exception.InnerException;
                throw;
            }
        }

        private class ExecutionStrategy
        {
            private delegate Task<ActivityResponse> ActivityMethod(ActivityExecutionMethod executionMethod, object targetInstance, object[] parameteres, string taskToken);

            private readonly ActivityMethod _activityMethod;
            private ExecutionStrategy(ActivityMethod activityMethod)
            {
                _activityMethod = activityMethod;
            }

            public async Task<ActivityResponse> Execute(ActivityExecutionMethod activityExecutionMethod, object targetInstance, object[] parameters, string taskToken)
            {
                return await _activityMethod(activityExecutionMethod, targetInstance, parameters, taskToken);
            }

            public static ExecutionStrategy CreateFor(MethodInfo executeMethod)
            {
                if (executeMethod.ReturnType == typeof(void))
                    return new ExecutionStrategy(VoidReturnType);
                if (executeMethod.ReturnType == typeof(Task))
                    return new ExecutionStrategy(TaskReturnType);
                if (executeMethod.ReturnType == typeof(ActivityResponse))
                    return new ExecutionStrategy(ActivityResponseReturnType);
                if (executeMethod.ReturnType.GetTypeInfo().IsGenericType && executeMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    if (executeMethod.ReturnType.GetGenericArguments()[0] == typeof(ActivityResponse))
                        return new ExecutionStrategy(TaskOfActivityResponseReturnType);
                    else
                        return new ExecutionStrategy(TaskOfGenericTypeReturnType);
                
                return new ExecutionStrategy(GenericTypeReturnType);
            }

            private static Task<ActivityResponse> VoidReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                targetMethod.Execute(targetInstance, parameters);
                return Task.FromResult(ActivityResponse.Defer);
            }
            private static async Task<ActivityResponse> TaskReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                await (Task)targetMethod.Execute(targetInstance, parameters);
                return ActivityResponse.Defer;
            }

            private static Task<ActivityResponse> ActivityResponseReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                return  Task.FromResult((ActivityResponse)targetMethod.Execute(targetInstance, parameters));
            }
            private static async Task<ActivityResponse> TaskOfActivityResponseReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                return await (Task<ActivityResponse>)targetMethod.Execute(targetInstance, parameters);
            }
            private static async Task<ActivityResponse> TaskOfGenericTypeReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                var task = (Task)targetMethod.Execute(targetInstance, parameters);
                await task;
                var result = task.GetType().GetProperty("Result").GetValue(task);
                if(result.Primitive())
                    return new ActivityCompletedResponse(result.ToString());
                return new ActivityCompletedResponse(result.ToJson());
            }
            private static Task<ActivityResponse> GenericTypeReturnType(ActivityExecutionMethod targetMethod, object targetInstance, object[] parameters, string taskToken)
            {
                var result = targetMethod.Execute(targetInstance, parameters);
                if (result.Primitive())
                    return Task.FromResult((ActivityResponse)new ActivityCompletedResponse(result.ToString()));

                return Task.FromResult((ActivityResponse)new ActivityCompletedResponse(result.ToJson()));
            }
        }
    }
}