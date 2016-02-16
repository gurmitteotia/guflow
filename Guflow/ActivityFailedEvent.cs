﻿using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityFailedEvent : ActivityEvent
    {
        private readonly ActivityTaskFailedEventAttributes _eventAttributes;

        public ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _eventAttributes = activityFailedHistoryEvent.ActivityTaskFailedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);

        }

        public string Reason { get { return _eventAttributes.Reason; } }
        public string Detail { get { return _eventAttributes.Details; } }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityFailed(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}