using System.Linq;

namespace Guflow.Decider
{
    public class Limit
    {
        private readonly uint _count;

        private Limit(uint count)
        {
            _count = count;
        }
        public static Limit Count(uint count)
        {
            return new Limit(count);
        }
        internal bool IsExceeded(WorkflowItem workflowItem)
        {
            var allEvents = workflowItem.AllEventsOf(workflowItem.LatestEvent.GetType());
            return allEvents.Count() > _count;
        }
    }
}