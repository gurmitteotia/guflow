using System;
using System.Threading.Tasks;

namespace Guflow.Worker
{
    public class ConcurrentExecution
    {
        private ConcurrentExecution(int maximumLimit)
        {
            
        }
        public static ConcurrentExecution LimitTo(int maximumLimit)
        {
        }

        internal void Execute(Func<Task> function)
        {
            throw new System.NotImplementedException();
        }

        internal void Set(HostedActivities hostedActivities)
        {
            throw new NotImplementedException();
        }
    }
}