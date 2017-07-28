using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowHistoryHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly long _newEventsStartId;
        private readonly long _newEventsEndId;

        public WorkflowHistoryHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, long newEventsStartId, long newEventsEndId)
        {
            _allHistoryEvents = allHistoryEvents;
            _newEventsStartId = newEventsStartId;
            _newEventsEndId = newEventsEndId;
        }

        public WorkflowHistoryHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
            :this(allHistoryEvents,allHistoryEvents.Last().EventId, allHistoryEvents.First().EventId)
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

        public IEnumerable<WorkflowDecision> InterpretNewEventsFor(IWorkflow workflow)
        {
            var interpretedWorkflowActions = new List<WorkflowAction>();

            for (var eventId = _newEventsStartId;eventId<=_newEventsEndId;eventId++)
            {
                var historyEvent = _allHistoryEvents.FirstOrDefault(h => h.EventId == eventId);
                if (historyEvent == null)
                    continue;
                var workflowEvent = historyEvent.CreateInterpretableEvent(_allHistoryEvents);
                if(workflowEvent!=null)
                    interpretedWorkflowActions.Add(workflowEvent.Interpret(workflow));
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
        public IEnumerable<WorkflowItemEvent> AllActivityEventsFor(ActivityItem activityItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateActivityEventFor(_allHistoryEvents);
                if (workflowItemEvent.IsFor(activityItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    allEvents.Add(workflowItemEvent);
                    yield return workflowItemEvent;
                }
            }
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
                    yield return workflowItemEvent;
                }
            }
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
