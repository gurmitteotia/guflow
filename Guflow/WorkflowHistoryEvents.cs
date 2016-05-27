using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
        }

        public ActivityEvent LatestActivityEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCompletedEvent())
                {
                    var completedActivityEvent = new ActivityCompletedEvent(historyEvent,_allHistoryEvents);
                    if (completedActivityEvent.IsFor(activityItem))
                        return completedActivityEvent;
                }

                else if (historyEvent.IsActivityFailedEvent())
                {
                    var failedActivityEvent = new ActivityFailedEvent(historyEvent, _allHistoryEvents);
                    if (failedActivityEvent.IsFor(activityItem))
                        return failedActivityEvent;
                }
                else if (historyEvent.IsActivityTimedoutEvent())
                {
                    var timedoutActivityEvent = new ActivityTimedoutEvent(historyEvent, _allHistoryEvents);
                    if (timedoutActivityEvent.IsFor(activityItem))
                        return timedoutActivityEvent;
                }
                else if (historyEvent.IsActivityCancelledEvent())
                {
                    var cancelledActivityEvent = new ActivityCancelledEvent(historyEvent, _allHistoryEvents);
                    if (cancelledActivityEvent.IsFor(activityItem))
                        return cancelledActivityEvent;
                }
            }

            return null;
        }

        public TimerFiredEvent LatestTimerEventFor(TimerItem timerItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsTimerFiredEvent())
                {
                    var firedTimerEvent = new TimerFiredEvent(historyEvent,_allHistoryEvents);
                    if(firedTimerEvent.IsFor(timerItem))
                        return firedTimerEvent;
                }
            }
            return null;
        }

        public IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow)
        {
            var workflowActions = new List<WorkflowAction>();

            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowEvent = historyEvent.CreateEventFor(_allHistoryEvents);
                if(workflowEvent!=null)
                    workflowActions.Add(workflowEvent.Interpret(workflow));
            }

            return workflowActions.SelectMany(a => a.GetDecisions());
        }
    }
}
