// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        /// <summary>
        /// Limit the rescheduling to giving number of counts.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Limit Count(uint count)
        {
            return new Limit(count);
        }
        internal bool IsExceeded(WorkflowItem workflowItem)
        {
            var allEvents = workflowItem.Events(workflowItem.LastEvent().GetType());
            return allEvents.Count() > _count;
        }
    }
}