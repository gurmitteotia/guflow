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
                    var activityCompletedEvent = new ActivityFailedEvent(historyEvent, _allHistoryEvents);
                    if (activityCompletedEvent.Has(activityName, activityVersion, positionalName))
                        return activityCompletedEvent;
                }
                else if (historyEvent.IsActivityTimedoutEvent())
                {
                    var activityCompletedEvent = new ActivityTimedoutEvent(historyEvent, _allHistoryEvents);
                    if (activityCompletedEvent.Has(activityName, activityVersion, positionalName))
                        return activityCompletedEvent;
                }

            }

            return null;
        }
    }
}
