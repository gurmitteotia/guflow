using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly long _newEventsStartId;
        private readonly long _newEventsEndId;

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, long newEventsStartId, long newEventsEndId)
        {
            _allHistoryEvents = allHistoryEvents;
            _newEventsStartId = newEventsStartId;
            _newEventsEndId = newEventsEndId;
        }

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
            :this(allHistoryEvents,allHistoryEvents.First().EventId, allHistoryEvents.Last().EventId)
        {
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

            for (var eventId = _newEventsStartId;eventId>=_newEventsEndId;eventId--)
            {
                var historyEvent = _allHistoryEvents.FirstOrDefault(h => h.EventId == eventId);
                if (historyEvent == null)
                    continue;
                var workflowEvent = historyEvent.CreateEventFor(_allHistoryEvents);
                if(workflowEvent!=null)
                    workflowActions.Add(workflowEvent.Interpret(workflow));
            }

            return workflowActions.SelectMany(a => a.GetDecisions()).Distinct().Where(d=>d!=WorkflowDecision.Empty);
        }

        public WorkflowStartedEvent WorkflowStartedEvent()
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if(historyEvent.IsWorkflowStartedEvent())
                    return new WorkflowStartedEvent(historyEvent);
            }
            throw new IncompleteEventGraphException("Can not find workflow started event.");
        }

        public bool IsActive()
        {
            throw new System.NotImplementedException();
        }
    }
}
