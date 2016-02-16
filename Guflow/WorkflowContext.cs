using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class WorkflowContext : IWorkflowContext
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;

        public WorkflowContext(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
        }

        public ActivityEvent GetActivityEvent(string activityName, string activityVersion, string positionalName = "")
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCompletedEvent())
                {
                    var activityCompletedEvent = new ActivityCompletedEvent(historyEvent,_allHistoryEvents);
                    if(activityCompletedEvent.Has(activityName,activityVersion,positionalName))
                        return activityCompletedEvent;
                }

                else if (historyEvent.IsActivityFailedEvent())
                {
                    var activityFailedEvent = new ActivityFailedEvent(historyEvent, _allHistoryEvents);
                    if (activityFailedEvent.Has(activityName, activityVersion, positionalName))
                        return activityFailedEvent;
                }
                else if (historyEvent.IsActivityTimedoutEvent())
                {
                    var activityTimedoutEvent = new ActivityTimedoutEvent(historyEvent, _allHistoryEvents);
                    if (activityTimedoutEvent.Has(activityName, activityVersion, positionalName))
                        return activityTimedoutEvent;
                }
                else if (historyEvent.IsActivityCancelledEvent())
                {
                    var activityCancelledEvent = new ActivityCancelledEvent(historyEvent, _allHistoryEvents);
                    if (activityCancelledEvent.Has(activityName, activityVersion, positionalName))
                        return activityCancelledEvent;
                }
            }

            return null;
        }
    }
}
