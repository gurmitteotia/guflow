// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal static class WaitForSignalEventsExtension
    {
        public static IEnumerable<WorkflowItem> WaitingItems(this IEnumerable<WaitForSignalsEvent> events, WorkflowItem[] workflowItems, string signalName)
        {
            var waitingItems = new List<WorkflowItem>();
            foreach (var waitEvent in events)
            {
                var items = workflowItems.Where(w => waitEvent.IsFor(w, signalName));
                waitingItems.AddRange(items);
            }
            return waitingItems;
        }

        public static WaitForSignalsEvent FirstOrDefault(this IEnumerable<WaitForSignalsEvent> events, WorkflowItem workflowItem, string signalName)
        {
            foreach (var waitEvent in events)
            {
                if (waitEvent.IsFor(workflowItem, signalName))
                    return waitEvent;
            }
            return null;
        }

        public static WaitForSignalsEvent FirstOrDefault(this IEnumerable<WaitForSignalsEvent> events, WorkflowItem workflowItem, long triggerEventId)
        {
            foreach (var waitEvent in events)
            {
                if (waitEvent.IsFor(workflowItem, triggerEventId))
                    return waitEvent;
            }
            return null;
        }

        public static WaitForSignalsEvent FirstOrDefault(this IEnumerable<WaitForSignalsEvent> events, WorkflowItem workflowItem)
        {
            foreach (var waitEvent in events)
            {
                if (waitEvent.IsFor(workflowItem))
                    return waitEvent;
            }

            return null;
        }

        public static WaitForSignalsEvent FirstOrDefault(this IEnumerable<WaitForSignalsEvent> @events,
            long timeoutTriggerEventId)
        {
            foreach(var waitEvent in @events)
            {
                if (waitEvent.HasTimedoutTriggerId(timeoutTriggerEventId))
                {
                    return waitEvent;
                }
            }

            return null;
        }
        private static bool IsFor(this WaitForSignalsEvent @event, WorkflowItem item, string signalName)
        {
            return @event.IsWaitingForSignal(signalName) && @event.IsFor(item);
        }

        private static bool IsFor(this WaitForSignalsEvent @event, WorkflowItem item, long triggerEventId)
        {
            return @event.TriggerEventId==triggerEventId && @event.IsFor(item);
        }
    }
}