﻿using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public abstract class ActivityEvent : WorkflowItemEvent
    {
        private string _activityName;
        private string _activityVersion;
        private string _activityPositionalName;
        private long _startedEventId;
        private long _scheduledEventId;
        public string WorkerIdentity { get; private set; }

        protected ActivityEvent(long eventId)
            : base(eventId)
        {
        }
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

        internal bool InChainOf(IEnumerable<ActivityEvent> activityEvents)
        {
            foreach (var itemEvent in activityEvents)
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