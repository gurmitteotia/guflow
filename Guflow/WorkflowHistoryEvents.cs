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

        public WorkflowItemEvent LastActivityEventFor(WorkflowItem activityItem)
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
                else if (historyEvent.IsActivityCancelRequestedEvent())
                {
                    var activityCancelRequestedEvent = new ActivityCancelRequestedEvent(historyEvent);
                    if (activityCancelRequestedEvent.IsFor(activityItem))
                        return activityCancelRequestedEvent;
                }
                else if (historyEvent.IsActivityCancellationFailedEvent())
                {
                    var activityCancellationFailedEvent = new ActivityCancellationFailedEvent(historyEvent);
                    if(activityCancellationFailedEvent.IsFor(activityItem))
                        return activityCancellationFailedEvent;
                }
                else if (historyEvent.IsActivityStartedEvent())
                {
                    var activityStartedEvent = new ActivityStartedEvent(historyEvent, _allHistoryEvents);
                    if (activityStartedEvent.IsFor(activityItem))
                        return activityStartedEvent;
                }
                else if (historyEvent.IsActivityScheduledEvent())
                {
                    var activityScheduledEvent = new ActivityScheduledEvent(historyEvent,_allHistoryEvents);
                    if (activityScheduledEvent.IsFor(activityItem))
                        return activityScheduledEvent;
                }
                else if (historyEvent.IsActivitySchedulingFailedEvent())
                {
                    var activitySchedulingFailedEvent = new ActivitySchedulingFailedEvent(historyEvent);
                    if (activitySchedulingFailedEvent.IsFor(activityItem))
                        return activitySchedulingFailedEvent;
                }
            }

            return WorkflowItemEvent.NotFound;
        }

        public WorkflowItemEvent LastTimerEventFor(WorkflowItem timerItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsTimerFiredEvent())
                {
                    var firedTimerEvent = new TimerFiredEvent(historyEvent,_allHistoryEvents);
                    if(firedTimerEvent.IsFor(timerItem))
                        return firedTimerEvent;
                }
                if (historyEvent.IsTimerStartedEvent())
                {
                    var timerStartedEvent = new TimerStartedEvent(historyEvent,_allHistoryEvents);
                    if (timerStartedEvent.IsFor(timerItem))
                        return timerStartedEvent;
                }
                if (historyEvent.IsTimerStartFailedEvent())
                {
                    var timerStartFailedEvent = new TimerStartFailedEvent(historyEvent);
                    if (timerStartFailedEvent.IsFor(timerItem))
                        return timerStartFailedEvent;
                }
                if (historyEvent.IsTimerCancelledEvent())
                {
                    var timerCancelledEvent = new TimerCancelledEvent(historyEvent,_allHistoryEvents);
                    if(timerCancelledEvent.IsFor(timerItem))
                        return timerCancelledEvent;
                }
                if (historyEvent.IsTimerCancellationFailedEvent())
                {
                    var timerCancellationFailedEvent = new TimerCancellationFailedEvent(historyEvent);
                    if (timerCancellationFailedEvent.IsFor(timerItem))
                        return timerCancellationFailedEvent;
                }
            }
            return WorkflowItemEvent.NotFound;
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

        public ActivityCompletedEvent LastCompletedEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCompletedEvent())
                {
                    var activityCompletedEvent = new ActivityCompletedEvent(historyEvent,_allHistoryEvents);
                    if (activityCompletedEvent.IsFor(activityItem))
                        return activityCompletedEvent;
                }
            }
            return null;
        }

        public ActivityFailedEvent LastFailedEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityFailedEvent())
                {
                    var activityFailedEvent = new ActivityFailedEvent(historyEvent, _allHistoryEvents);
                    if (activityFailedEvent.IsFor(activityItem))
                        return activityFailedEvent;
                }
            }
            return null;
        }
        public ActivityTimedoutEvent LastTimedoutEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityTimedoutEvent())
                {
                    var activityTimedoutEvent = new ActivityTimedoutEvent(historyEvent, _allHistoryEvents);
                    if (activityTimedoutEvent.IsFor(activityItem))
                        return activityTimedoutEvent;
                }
            }
            return null;
        }

        public ActivityCancelledEvent LastCancelledEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCancelledEvent())
                {
                    var activityCancelledEvent = new ActivityCancelledEvent(historyEvent, _allHistoryEvents);
                    if (activityCancelledEvent.IsFor(activityItem))
                        return activityCancelledEvent;
                }
            }
            return null;
        }

        public IEnumerable<WorkflowItemEvent> AllEventsFor(ActivityItem activityItem)
        {
            var activityEvents = new List<ActivityEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsActivityCompletedEvent())
                {
                    var activityCompletedEvent = new ActivityCompletedEvent(historyEvent,_allHistoryEvents);
                    if(activityCompletedEvent.IsFor(activityItem))
                        activityEvents.Add(activityCompletedEvent);
                }
                else if (historyEvent.IsActivityStartedEvent())
                {
                    var activityStartedEvent = new ActivityStartedEvent(historyEvent, _allHistoryEvents);
                    if (activityStartedEvent.IsFor(activityItem) && !activityStartedEvent.InChainOf(activityEvents))
                        activityEvents.Add(activityStartedEvent);
                }
                else if (historyEvent.IsActivityScheduledEvent())
                {
                    var activityScheduledEvent = new ActivityScheduledEvent(historyEvent,_allHistoryEvents);
                    if(activityScheduledEvent.IsFor(activityItem) && !activityScheduledEvent.InChainOf(activityEvents))
                        activityEvents.Add(activityScheduledEvent);
                }
            }
            return activityEvents;
        }
    }
}
