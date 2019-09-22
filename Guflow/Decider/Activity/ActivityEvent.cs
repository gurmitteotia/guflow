// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Base class for activity events.
    /// </summary>
    public abstract class ActivityEvent : WorkflowItemEvent
    {
        private string _activityName;
        private string _activityVersion;
        private string _activityPositionalName;
        private long _startedEventId;
        private long _scheduledEventId;
        /// <summary>
        /// Returns the worker polling identity.
        /// </summary>
        public string WorkerIdentity { get; private set; }

        /// <summary>
        /// Returns the input activity was scheduled with.
        /// </summary>
        public string Input { get; private set; }
        protected ActivityEvent(HistoryEvent historyEvent)
            : base(historyEvent)
        {
        }
        /// <summary>
        /// For internal use only.
        /// </summary>
        /// <param name="allHistoryEvents"></param>
        /// <param name="startedEventId"></param>
        /// <param name="scheduledEventId"></param>
        protected void PopulateActivityFrom(IEnumerable<HistoryEvent> allHistoryEvents, long startedEventId, long scheduledEventId)
        {
            bool foundActivityScheduledEvent=false;
            _startedEventId = startedEventId;
            _scheduledEventId = scheduledEventId;
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsActivityStartedEvent(startedEventId))
                {
                    WorkerIdentity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                }
                else if (historyEvent.IsActivityScheduledEvent(scheduledEventId))
                {
                    var attr = historyEvent.ActivityTaskScheduledEventAttributes;
                    _activityName = attr.ActivityType.Name;
                    _activityVersion = attr.ActivityType.Version;
                    _activityPositionalName = attr.Control.As<ScheduleData>().PN;
                    ScheduleId = ScheduleId.Raw(attr.ActivityId);
                    Input = attr.Input;
                    foundActivityScheduledEvent = true;
                }
            }
            if (!foundActivityScheduledEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity scheduled event id {0}.", scheduledEventId));
        }

        public override string ToString()
        {
            return $"{GetType().Name} for activity name {_activityName}, version {_activityVersion} and positional name {_activityPositionalName}";
        }

        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            foreach (var itemEvent in workflowItemEvents.OfType<ActivityEvent>())
            {
                if (IsInChainOf(itemEvent))
                    return true;
            }
            return false;
        }
        private bool IsInChainOf(ActivityEvent otherActivityEvent)
        {
            return _startedEventId == otherActivityEvent._startedEventId ||
                   _scheduledEventId == otherActivityEvent._scheduledEventId;
        }
    }
}