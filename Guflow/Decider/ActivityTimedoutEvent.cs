﻿using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class ActivityTimedoutEvent : ActivityEvent
    {
        private readonly ActivityTaskTimedOutEventAttributes _eventAttributes;
        internal ActivityTimedoutEvent(HistoryEvent activityTimedoutEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityTimedoutEvent.EventId)
        {
            _eventAttributes = activityTimedoutEvent.ActivityTaskTimedOutEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        public string TimeoutType { get { return _eventAttributes.TimeoutType; } }

        public string Details { get { return _eventAttributes.Details; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.OnActivityTimeout(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            var details = string.IsNullOrEmpty(Details) ? "ActivityTimedout" : Details;
            return defaultActions.FailWorkflow(TimeoutType, details);
        }
    }
}