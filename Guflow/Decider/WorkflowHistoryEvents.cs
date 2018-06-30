﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly long _newEventsStartId;
        private readonly long _newEventsEndId;
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedActivityEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedTimerEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();
        private readonly Dictionary<WorkflowItem,WorkflowItemEvent> _cachedLambdaEvents = new Dictionary<WorkflowItem, WorkflowItemEvent>();

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents, long newEventsStartId, long newEventsEndId)
        {
            _allHistoryEvents = allHistoryEvents;
            _newEventsStartId = newEventsStartId;
            _newEventsEndId = newEventsEndId;
        }

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
            :this(allHistoryEvents,allHistoryEvents.Last().EventId, allHistoryEvents.First().EventId)
        {
        }

        public WorkflowItemEvent LastActivityEvent(ActivityItem activityItem)
        {
            WorkflowItemEvent result = null;
            if (_cachedActivityEvents.TryGetValue(activityItem, out result)) return result;

            result = AllActivityEvents(activityItem).FirstOrDefault();
           _cachedActivityEvents.Add(activityItem, result);
            return result;
        }

        public WorkflowItemEvent LastTimerEvent(TimerItem timerItem)
        {
            WorkflowItemEvent result = null;
            if (_cachedTimerEvents.TryGetValue(timerItem, out result)) return result;

            result = AllTimerEvents(timerItem).FirstOrDefault();
            _cachedTimerEvents.Add(timerItem, result);
            return result;
        }

        //TODO: Move this code outside of this class.
        public IEnumerable<WorkflowDecision> InterpretNewEvents(IWorkflow workflow)
        {
            var result = new List<WorkflowAction>();
            
            foreach(var workflowEvent in NewEvents())
            { 
                workflow.SetCurrentExecutingEvent(workflowEvent);
                result.Add(workflowEvent.Interpret(workflow));
            }
            return result.Where(w=>w!=null).SelectMany(a => a.Decisions()).Distinct();
        }

        public IEnumerable<WorkflowEvent> NewEvents()
        {
            var events = new List<WorkflowEvent>();
            for (var eventId = _newEventsStartId; eventId <= _newEventsEndId; eventId++)
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
        public IEnumerable<WorkflowItemEvent> AllActivityEvents(ActivityItem activityItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateActivityEventFor(_allHistoryEvents);
                if (workflowItemEvent == null) continue;
                if (workflowItemEvent.IsFor(activityItem) && !workflowItemEvent.InChainOf(allEvents))
                {
                    allEvents.Add(workflowItemEvent);
                    yield return workflowItemEvent;
                }
            }
        }

        public IEnumerable<WorkflowItemEvent> AllTimerEvents(TimerItem timerItem)
        {
            var allEvents = new List<WorkflowItemEvent>();
            foreach (var historyEvent in _allHistoryEvents)
            {
                var workflowItemEvent = historyEvent.CreateTimerEventFor(_allHistoryEvents);
                if (workflowItemEvent == null) continue;
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
            @event = AllLambdaEvents(lambdaItem).FirstOrDefault();
            _cachedLambdaEvents.Add(lambdaItem, @event);
            return @event;
        }

        public long LatestEventId => _allHistoryEvents.First().EventId;

    }
}
