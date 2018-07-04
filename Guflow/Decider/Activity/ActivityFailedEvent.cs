// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when activity has failed. By default workflow will be failed when it process the activity failed event.
    /// </summary>
    public class ActivityFailedEvent : ActivityEvent
    {
        private readonly ActivityTaskFailedEventAttributes _eventAttributes;

        internal ActivityFailedEvent(HistoryEvent activityFailedHistoryEvent, IEnumerable<HistoryEvent> allHistoryEvents) : base(activityFailedHistoryEvent.EventId)
        {
            _eventAttributes = activityFailedHistoryEvent.ActivityTaskFailedEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        /// <summary>
        /// Returns reason why activity has failed.
        /// </summary>
        public string Reason => _eventAttributes.Reason;
        /// <summary>
        /// Returns details for activity failure.
        /// </summary>
        public string Details => _eventAttributes.Details;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow(Reason, Details);
        }
    }
}
