using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public static class WorkflowItemExtensions
    {
        public static IEnumerable<WorkflowItemEvent> AllEventsOf(this IWorkflowItem workflowItem, Type eventType)
        {
            return workflowItem.AllEvents.Where(e => e.GetType() == eventType);
        }

        public static IEnumerable<WorkflowItemEvent> AllEventsOf<TEvent>(this IWorkflowItem workflowItem) where TEvent : WorkflowItemEvent
        {
            return workflowItem.AllEvents.OfType<TEvent>();
        }
    }
}