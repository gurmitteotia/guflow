// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when scheduling for lamdba function failed in SWF.
    /// </summary>
    public class LambdaSchedulingFailedEvent : WorkflowItemEvent
    {
        internal LambdaSchedulingFailedEvent(HistoryEvent failedEvent) : base(failedEvent.EventId)
        {
            var attr = failedEvent.ScheduleLambdaFunctionFailedEventAttributes;
            AwsIdentity = AwsIdentity.Raw(attr.Id);
            Cause = attr.Cause?.Value;
        }

        /// <summary>
        /// Return cause on why scheduling of lambda function has failed in SWF.
        /// </summary>
        public string Cause { get; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }
    }
}