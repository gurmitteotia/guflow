// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        protected ActivityEvent(long eventId)
            : base(eventId)
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
                if (historyEvent.IsActivityStartedEventFor(startedEventId))
                {
                    WorkerIdentity = historyEvent.ActivityTaskStartedEventAttributes.Identity;
                }
                else if (historyEvent.IsActivityScheduledEventFor(scheduledEventId))
                {
                    _activityName = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Name;
                    _activityVersion = historyEvent.ActivityTaskScheduledEventAttributes.ActivityType.Version;
                    _activityPositionalName = historyEvent.ActivityTaskScheduledEventAttributes.Control.FromJson<ActivityScheduleData>().PN;
                    AwsIdentity = AwsIdentity.Raw(historyEvent.ActivityTaskScheduledEventAttributes.ActivityId);
                    Input = historyEvent.ActivityTaskScheduledEventAttributes.Input;
                    foundActivityScheduledEvent = true;
                }
            }
            if (!foundActivityScheduledEvent)
                throw new IncompleteEventGraphException(string.Format("Can not found activity scheduled event id {0}.", scheduledEventId));
        }

        public override string ToString()
        {
            return string.Format("{0} for activity name {1}, version {2} and positional name {3}", GetType().Name, _activityName, _activityVersion, _activityPositionalName);
        }

        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            foreach (var itemEvent in workflowItemEvents.OfType<ActivityEvent>())
            {
                if (IsInChainOf(itemEvent))
                    return true;
            }
            //swf does not link cancel requested/failed event with scheduled id or start id
            foreach (var itemEvent in workflowItemEvents.OfType<ActivityCancelRequestedEvent>())
            {
                if (itemEvent.IsForSameWorkflowItemAs(this))
                    return true;
            }
            foreach (var itemEvent in workflowItemEvents.OfType<ActivityCancellationFailedEvent>())
            {
                if (itemEvent.IsForSameWorkflowItemAs(this))
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