// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly long _previousStartedEventId;
        private readonly long _newStartedEventId;
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedActivityEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedTimerEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedLambdaEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedChildWorkflowEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();

        private List<WaitForSignalsEvent> _cachedWaitEvents = null;
        //TODO : Get rid of this constructor once the dependent constructor is deleted.
        private WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, long previousStartedEventId, long newStartedEventId)
        {
            _allHistoryEvents = allHistoryEvents;
            _previousStartedEventId = previousStartedEventId;
            _newStartedEventId = newStartedEventId;
        }

        public WorkflowHistoryEvents(DecisionTask decisionTask)
        {
            _allHistoryEvents = decisionTask.Events;
            _previousStartedEventId = decisionTask.PreviousStartedEventId;
            _newStartedEventId = decisionTask.StartedEventId;
            WorkflowRunId = decisionTask.WorkflowExecution.RunId;
            WorkflowId = decisionTask.WorkflowExecution.WorkflowId;
        }

        //TODO: Get rid of this constructor.
        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, string workflowRunId="")
            :this(allHistoryEvents,allHistoryEvents.Last().EventId-1, allHistoryEvents.First().EventId)
        {
            WorkflowRunId = workflowRunId;
        }

        public WorkflowItemEvent LastActivityEvent(ActivityItem activityItem)
        {
            WorkflowItemEvent result = null;
            if (_cachedActivityEvents.TryGetValue(activityItem, out result)) return result;

            result = AllActivityEvents(activityItem).FirstOrDefault(e=>!LastEventFilters.Activity.Contains(e));
           _cachedActivityEvents.Add(activityItem, result);
            return result;
        }

        public WorkflowItemEvent LastTimerEvent(TimerItem timerItem, bool includeRescheduleTimerEvents)
        {
            WorkflowItemEvent result = null;
            if (_cachedTimerEvents.TryGetValue(timerItem, out result)) return result;

            result = AllTimerEvents(timerItem, includeRescheduleTimerEvents).FirstOrDefault(e => !LastEventFilters.Timer.Contains(e));
            _cachedTimerEvents.Add(timerItem, result);
            return result;
        }
        public IEnumerable<WorkflowEvent> NewEvents()
        {
            var events = new List<WorkflowEvent>();
            for (var eventId = _previousStartedEventId + 1; eventId <= _newStartedEventId; eventId++)
            {
                var historyEvent = _allHistoryEvents.First(h => h.EventId == eventId);
                var workflowEvent = historyEvent.CreateInterpretableEvent(_allHistoryEvents);
                if (workflowEvent == null)
                    continue;
                events.Add(workflowEvent);
            }
            return events;
        }

        public WorkflowStartedEvent WorkflowStartedEvent()
        {
            foreach (var historyEvent in _allHistoryEvents.Reverse())
            {
                if(historyEvent.IsWorkflowStartedEvent())
                    return new WorkflowStartedEvent(historyEvent);
            }
            throw new IncompleteEventGraphException("Can not find workflow started event.");
        }

        public bool HasActiveEvent()
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateWorkflowItemEventFor(_allHistoryEvents);
                if(workflowItemEvent == null) continue;
                if (workflowItemEvent.IsActive && !workflowItemEvent.InChainOf(allEvents))
                    return true;
                allEvents.Add(workflowItemEvent);
            }
            return false;
        }

        public string WorkflowRunId { get; }
        public string WorkflowId { get; }

        public IEnumerable<WorkflowItemEvent> AllActivityEvents(ActivityItem activityItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.ActivityEvent(_allHistoryEvents);
                if (workflowItemEvent == null) continue;
                if (workflowItemEvent.IsFor(activityItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    allEvents.Add(workflowItemEvent);
                    yield return workflowItemEvent;
                }
            }
        }

        public IEnumerable<WorkflowItemEvent> AllTimerEvents(TimerItem timerItem, bool includeRescheduleTimerEvents)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.TimerEvent(_allHistoryEvents);
                if (workflowItemEvent == null) continue;
                if (workflowItemEvent.IsFor(timerItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    if(!includeRescheduleTimerEvents && IsRescheduleTimerEvent(workflowItemEvent))
                        continue;
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

        public IEnumerable<WorkflowItemEvent> AllLambdaEvents(LambdaItem lambdaItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var lambdaEvent = historyEvent.LambdaEvent(_allHistoryEvents);
                if(lambdaEvent == null) continue;
                if (lambdaEvent.IsFor(lambdaItem) && !lambdaEvent.InChainOf(allEvents))
                {
                    allEvents.Add(lambdaEvent);
                    yield return lambdaEvent;
                }
            }
        }

        public WorkflowItemEvent LastLambdaEvent(LambdaItem lambdaItem)
        {
            WorkflowItemEvent @event = null;
            if (_cachedLambdaEvents.TryGetValue(lambdaItem, out @event))
                return @event;
            @event = AllLambdaEvents(lambdaItem).FirstOrDefault(e=>!LastEventFilters.Lambda.Contains(e));
            _cachedLambdaEvents.Add(lambdaItem, @event);
            return @event;
        }

        public IEnumerable<WorkflowItemEvent> AllChildWorkflowEvents(ChildWorkflowItem childWorkflowItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var childWorkflowEvent = historyEvent.ChildWorkflowEvent(_allHistoryEvents);
                if (childWorkflowEvent == null) continue;
                if (childWorkflowEvent.IsFor(childWorkflowItem) && !childWorkflowEvent.InChainOf(allEvents))
                {
                    allEvents.Add(childWorkflowEvent);
                    yield return childWorkflowEvent;
                }
            }
        }

        public WorkflowItemEvent LastChildWorkflowEvent(ChildWorkflowItem childWorkflowItem)
        {
            WorkflowItemEvent @event = null;
            if (_cachedChildWorkflowEvents.TryGetValue(childWorkflowItem, out @event))
                return @event;
            @event = AllChildWorkflowEvents(childWorkflowItem).FirstOrDefault(e=>!LastEventFilters.ChildWorkflow.Contains(e));
            _cachedChildWorkflowEvents.Add(childWorkflowItem, @event);
            return @event;
        }
        public IEnumerable<WaitForSignalsEvent> WaitForSignalsEvents()
        {
            if (_cachedWaitEvents != null) return _cachedWaitEvents;
            _cachedWaitEvents = new List<WaitForSignalsEvent>();
            foreach (var historyEvent in _allHistoryEvents.Reverse())
            {
                var waitEvent = historyEvent.WaitForSignalsEvent(_allHistoryEvents);
                if (waitEvent != null)
                    _cachedWaitEvents.Add(waitEvent);
            }

            return _cachedWaitEvents;
        }

        public long LatestEventId => _allHistoryEvents.First().EventId;

        private bool IsRescheduleTimerEvent(WorkflowItemEvent @event)
        {
            var timerEvent = @event as TimerEvent;
            return timerEvent != null && timerEvent.IsARescheduledTimer;
        }

        private class LastEventFilters
        {
            private readonly Type[] _filterOutTypes;

            private LastEventFilters(params Type[] filterOutTypes)
            {
                _filterOutTypes = filterOutTypes;
            }
            public static readonly LastEventFilters Activity =
                new LastEventFilters(typeof(ActivityCancelRequestedEvent), typeof(ActivityCancellationFailedEvent), typeof(ActivitySchedulingFailedEvent));
            public static readonly LastEventFilters ChildWorkflow =
                new LastEventFilters(typeof(ExternalWorkflowCancelRequestFailedEvent), typeof(ExternalWorkflowCancellationRequestedEvent), typeof(ChildWorkflowStartFailedEvent));
            public static readonly LastEventFilters Timer =
                new LastEventFilters(typeof(TimerStartFailedEvent), typeof(TimerCancellationFailedEvent));
            public static readonly LastEventFilters Lambda =
                new LastEventFilters(typeof(LambdaSchedulingFailedEvent));

            public bool Contains(WorkflowItemEvent @event)
                => _filterOutTypes.Contains(@event.GetType());
        }
    }
}
