// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Indicates that activity has timed out. By default it will cause the workflow to fail.
    /// </summary>
    public class ActivityTimedoutEvent : ActivityEvent
    {
        private readonly ActivityTaskTimedOutEventAttributes _eventAttributes;
        internal ActivityTimedoutEvent(HistoryEvent activityTimedoutEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityTimedoutEvent.EventId)
        {
            _eventAttributes = activityTimedoutEvent.ActivityTaskTimedOutEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }
        /// <summary>
        /// Returns reason, why activity has timedout.
        /// </summary>
        public string TimeoutType => _eventAttributes.TimeoutType;

        /// <summary>
        /// Returns last heartbeat reported details.
        /// </summary>
        public string Details => _eventAttributes.Details;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            var details = string.IsNullOrEmpty(Details) ? "ActivityTimedout" : Details;
            return defaultActions.FailWorkflow(TimeoutType, details);
        }
    }
}