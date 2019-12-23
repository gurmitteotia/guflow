// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly WorkflowTask _workflowTask;

        private List<WaitForSignalsEvent> _cachedWaitEvents = null;
        //TODO : Get rid of this constructor once the dependent constructor is deleted.
        private WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, long previousStartedEventId,
            long newStartedEventId, string workflowRunId)
        {
            var decision = new DecisionTask()
            {
                PreviousStartedEventId = previousStartedEventId, StartedEventId = newStartedEventId, WorkflowExecution =
                    new WorkflowExecution()
                    {
                        RunId = workflowRunId
                    }
            };
            decision.Events = allHistoryEvents.ToList();
            decision.TaskToken = "dummy";
            _workflowTask = WorkflowTask.Create(decision);
        }

        public WorkflowHistoryEvents(WorkflowTask workflowTask)
        {
            _workflowTask = workflowTask;
        }

        //TODO: Get rid of this constructor.
        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, string workflowRunId="")
            :this(allHistoryEvents,allHistoryEvents.Last().EventId-1, allHistoryEvents.First().EventId, workflowRunId)
        {
        }

        public WorkflowItemEvent LastActivityEvent(ActivityItem activityItem)
        {
            return  AllActivityEvents(activityItem).FirstOrDefault(e=>!LastEventFilters.Activity.Contains(e));
        }

        public WorkflowItemEvent LastTimerEvent(TimerItem timerItem, bool includeRescheduleTimerEvents)
        {
            return AllTimerEvents(timerItem, includeRescheduleTimerEvents).FirstOrDefault(e => !LastEventFilters.Timer.Contains(e));
        }
        public IEnumerable<WorkflowEvent> NewEvents()
        {
            var events = new List<WorkflowEvent>();
            foreach (var historyEvent in _workflowTask.NewEvents)
            {
                var workflowEvent = historyEvent.CreateInterpretableEvent(_workflowTask.AllEvents);
                if (workflowEvent == null)
                    continue;
                events.Add(workflowEvent);
            }
            return events;
        }

        public WorkflowEvent CurrentExecutingEvent { get; set; }

        public WorkflowStartedEvent WorkflowStartedEvent()
        {
            foreach (var historyEvent in _workflowTask.AllEventsInAscOrderOfEventId)
            {
                if(historyEvent.IsWorkflowStartedEvent())
                    return new WorkflowStartedEvent(historyEvent);
            }
            throw new IncompleteEventGraphException("Can not find workflow started event.");
        }

        public bool HasActiveEvent()
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _workflowTask.AllEvents)
            {
                var workflowItemEvent = historyEvent.CreateWorkflowItemEventFor(_workflowTask.AllEvents);
                if(workflowItemEvent == null) continue;
                if (workflowItemEvent.IsActive && !workflowItemEvent.InChainOf(allEvents))
                    return true;
                allEvents.Add(workflowItemEvent);
            }
            return false;
        }

        public string WorkflowRunId => _workflowTask.RunId;
        public string WorkflowId => _workflowTask.WorkflowId;
        public DateTime ServerTimeUtc => _workflowTask.ServerTimeUtc;

        public IEnumerable<WorkflowItemEvent> AllActivityEvents(ActivityItem activityItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                var workflowItemEvent = historyEvent.ActivityEvent(_workflowTask.AllEvents);
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
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                var workflowItemEvent = historyEvent.TimerEvent(_workflowTask.AllEvents);
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
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                if(historyEvent.IsMarkerRecordedEvent())
                    yield return new MarkerRecordedEvent(historyEvent);
            }
        }

        public IEnumerable<WorkflowSignaledEvent> AllSignalEvents()
        {
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                if(historyEvent.IsWorkflowSignaledEvent())
                    yield return new WorkflowSignaledEvent(historyEvent);
            }
        }

        public IEnumerable<WorkflowCancellationRequestedEvent> AllWorkflowCancellationRequestedEvents()
        {
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                if (historyEvent.IsWorkflowCancellationRequestedEvent())
                    yield return new WorkflowCancellationRequestedEvent(historyEvent);
            }
        }

        public IEnumerable<WorkflowItemEvent> AllLambdaEvents(LambdaItem lambdaItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                var lambdaEvent = historyEvent.LambdaEvent(_workflowTask.AllEvents);
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
            return AllLambdaEvents(lambdaItem).FirstOrDefault(e=>!LastEventFilters.Lambda.Contains(e));
        }

        public IEnumerable<WorkflowItemEvent> AllChildWorkflowEvents(ChildWorkflowItem childWorkflowItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in AllEventsInDescOrderOfEventId)
            {
                var childWorkflowEvent = historyEvent.ChildWorkflowEvent(_workflowTask.AllEvents);
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
            return AllChildWorkflowEvents(childWorkflowItem).FirstOrDefault(e=>!LastEventFilters.ChildWorkflow.Contains(e));
        }
        public IEnumerable<WaitForSignalsEvent> WaitForSignalsEvents()
        {
            if (_cachedWaitEvents != null) return _cachedWaitEvents;
            _cachedWaitEvents = new List<WaitForSignalsEvent>();
            foreach (var historyEvent in AllEventsInAscOrderOfEventId)
            {
                var waitEvent = historyEvent.WaitForSignalsEvent(AllEventsInDescOrderOfEventId);
                if (waitEvent != null)
                    _cachedWaitEvents.Add(waitEvent);
            }

            return _cachedWaitEvents;
        }

        public long LatestEventId => _workflowTask.AllEvents.First().EventId;

        private IEnumerable<HistoryEvent> AllEventsInDescOrderOfEventId
        {
            get
            {
                long executingEventId = CurrentExecutingEvent != null ? CurrentExecutingEvent.EventId : long.MaxValue;
                return _workflowTask.AllEvents.SkipWhile(h => h.EventId >executingEventId);
            }
        }

        private IEnumerable<HistoryEvent> AllEventsInAscOrderOfEventId
        {
            get
            {
                long executingEventId = CurrentExecutingEvent != null ? CurrentExecutingEvent.EventId : long.MaxValue;
                return _workflowTask.AllEventsInAscOrderOfEventId.TakeWhile(h => h.EventId <= executingEventId);
            }
        }
        private static bool IsRescheduleTimerEvent(WorkflowItemEvent @event)
        {
            var timerEvent = @event as TimerEvent;
            return timerEvent != null && timerEvent.TimerType==TimerType.Reschedule;
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
