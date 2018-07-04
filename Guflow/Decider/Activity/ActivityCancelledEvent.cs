// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when activity is cancelled.
    /// </summary>
    public class ActivityCancelledEvent : ActivityEvent
    {
        private readonly ActivityTaskCanceledEventAttributes _eventAttributes;
        internal ActivityCancelledEvent(HistoryEvent cancelledActivityEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(cancelledActivityEvent.EventId)
        {
            _eventAttributes = cancelledActivityEvent.ActivityTaskCanceledEventAttributes;
            PopulateActivityFrom(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }
        /// <summary>
        /// Returns last details status of reported by activity heartbeat.
        /// </summary>
        public string Details => _eventAttributes.Details;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal bool IsCancelledEventFor(long cancelRequestedEventId)
        {
            return _eventAttributes.LatestCancelRequestedEventId == cancelRequestedEventId;
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.CancelWorkflow(Details);
        }
    }
}