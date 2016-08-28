using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public WorkflowItemEvent LastActivityEventFor(ActivityItem activityItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateActivityEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsFor(activityItem))
                    return workflowItemEvent;
            }

            return WorkflowItemEvent.NotFound;
        }

        public WorkflowItemEvent LastTimerEventFor(TimerItem timerItem)
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateTimerEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsFor(timerItem))
                    return workflowItemEvent;
            }
            return WorkflowItemEvent.NotFound;
        }

        public IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflowActions workflowActions)
        {
            var interpretedWorkflowActions = new List<WorkflowAction>();

            for (var eventId = _newEventsStartId;eventId>=_newEventsEndId;eventId--)
            {
                var historyEvent = _allHistoryEvents.FirstOrDefault(h => h.EventId == eventId);
                if (historyEvent == null)
                    continue;
                var workflowEvent = historyEvent.CreateInterpretableEvent(_allHistoryEvents);
                if(workflowEvent!=null)
                    interpretedWorkflowActions.Add(workflowEvent.Interpret(workflowActions));
            }

            return interpretedWorkflowActions.Where(w=>w!=null).SelectMany(a => a.GetDecisions()).Distinct();
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
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateWorkflowItemEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsActive && !workflowItemEvent.InChainOf(allEvents))
                    return true;
                allEvents.Add(workflowItemEvent);
            }
            return false;
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

        public IEnumerable<WorkflowItemEvent> AllActivityEventsFor(ActivityItem activityItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateActivityEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsFor(activityItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    allEvents.Add(workflowItemEvent);
                }
            }
            return allEvents;
        }

        public IEnumerable<WorkflowItemEvent> AllTimerEventsFor(TimerItem timerItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateTimerEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsFor(timerItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    allEvents.Add(workflowItemEvent);
                }
            }
            return allEvents;
        }

        public IEnumerable<MarkerRecordedEvent> AllMarkerRecordedEvents()
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if(historyEvent.IsMarkerRecordedEvent())
                    yield return new MarkerRecordedEvent(historyEvent);
            }
        }

        public IEnumerable<WorkflowSignaledEvent> AllSignalEvents()
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if(historyEvent.IsWorkflowSignaledEvent())
                    yield return new WorkflowSignaledEvent(historyEvent);
            }
        }

        public IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents()
        {
            foreach (var historyEvent in _allHistoryEvents)
            {
                if (historyEvent.IsWorkflowCancellationRequestedEvent())
                    yield return new WorkflowCancellationRequestedEvent(historyEvent);
            }
        }
    }
}
