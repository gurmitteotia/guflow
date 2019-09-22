// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class RecordMarkerFailedEvent : WorkflowEvent
    {
        private readonly RecordMarkerFailedEventAttributes _eventAttributes;
        internal RecordMarkerFailedEvent(HistoryEvent recordMarkerFailedEvent)
            : base(recordMarkerFailedEvent)
        {
            _eventAttributes = recordMarkerFailedEvent.RecordMarkerFailedEventAttributes;
        }

        public string MarkerName { get { return _eventAttributes.MarkerName; } }
        public string Cause { get { return _eventAttributes.Cause; } }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.FailWorkflow("FAILED_TO_RECORD_MARKER", Cause);
        }
    }
}