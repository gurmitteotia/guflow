using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowContext : IWorkflowContext
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;

        public WorkflowContext(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
        }

        public ActivityEvent LatestActivityEventFor(Identity identity)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCompletedEvent())
                {
                    var completedActivityEvent = new ActivityCompletedEvent(historyEvent,_allHistoryEvents);
                    if(completedActivityEvent.IsFor(identity))
                        return completedActivityEvent;
                }

                else if (historyEvent.IsActivityFailedEvent())
                {
                    var failedActivityEvent = new ActivityFailedEvent(historyEvent, _allHistoryEvents);
                    if (failedActivityEvent.IsFor(identity))
                        return failedActivityEvent;
                }
                else if (historyEvent.IsActivityTimedoutEvent())
                {
                    var timedoutActivityEvent = new ActivityTimedoutEvent(historyEvent, _allHistoryEvents);
                    if (timedoutActivityEvent.IsFor(identity))
                        return timedoutActivityEvent;
                }
                else if (historyEvent.IsActivityCancelledEvent())
                {
                    var cancelledActivityEvent = new ActivityCancelledEvent(historyEvent, _allHistoryEvents);
                    if (cancelledActivityEvent.IsFor(identity))
                        return cancelledActivityEvent;
                }
            }

            return null;
        }

        public TimerFiredEvent LatestTimerEventFor(Identity identity)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsTimerFiredEvent())
                {
                    var firedTimerEvent = new TimerFiredEvent(historyEvent,_allHistoryEvents);
                    if(firedTimerEvent.IsFor(identity))
                        return firedTimerEvent;
                }
            }
            return null;
        }
    }
}
