// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent an activity completed event. By default workflow execution will continue in processing the <see cref="ActivityCompletedEvent"/>.
    /// </summary>
    public class ActivityCompletedEvent : ActivityEvent
    {
        private readonly ActivityTaskCompletedEventAttributes _eventAttributes;
        internal ActivityCompletedEvent(HistoryEvent activityCompletedEvent, IEnumerable<HistoryEvent> allHistoryEvents)
            :base(activityCompletedEvent)
        {
            _eventAttributes = activityCompletedEvent.ActivityTaskCompletedEventAttributes;
            PopulateAttributes(allHistoryEvents, _eventAttributes.StartedEventId, _eventAttributes.ScheduledEventId);
        }

        /// <summary>
        /// Returns activity completed result.
        /// </summary>
        public string Result => _eventAttributes.Result;

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);            
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.Continue(this);
        }
    }
}