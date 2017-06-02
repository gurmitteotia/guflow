using System.Reflection;
using System.Threading;

namespace Guflow.Decider
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
            var parameters = _methodInfo.BuildParametersFrom(argument, default(CancellationToken));

            try
            {
                var returnType = _methodInfo.Invoke(_targetInstance, parameters);
                return returnType == null ? WorkflowAction.Ignore(true) : (WorkflowAction)returnType;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }
    }
}