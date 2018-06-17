// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Reported when scheduled lambda function is successfully completed.
    /// </summary>
    public class LamdbaFunctionCompletedEvent : WorkflowItemEvent
    {
        internal LamdbaFunctionCompletedEvent(HistoryEvent completedEvent, IEnumerable<HistoryEvent> eventGraph) : base(completedEvent.EventId)
        {
            var attributes = completedEvent.LambdaFunctionCompletedEventAttributes;
            Result = attributes.Result;
            PopulateProperties(attributes.ScheduledEventId, eventGraph);
        }

        /// <summary>
        /// Returns the lambda function name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the result of successfully completed lambda function.
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Returns the input for lamdba function.
        /// </summary>
        public string Input { get; private set; }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }

        private void PopulateProperties(long scheduledEventId, IEnumerable<HistoryEvent> eventGraph)
        {
            foreach (var historyEvent in eventGraph)
            {
                if (historyEvent.IsLambdaScheduledEvent(scheduledEventId))
                {
                    Input = historyEvent.LambdaFunctionScheduledEventAttributes.Input;
                    Name = historyEvent.LambdaFunctionScheduledEventAttributes.Name;
                    AwsIdentity = AwsIdentity.Raw(historyEvent.LambdaFunctionScheduledEventAttributes.Id);
                    break;
                }
            }
            if(AwsIdentity == null)
                throw new IncompleteEventGraphException($"Can not find lambda scheduled event for id {scheduledEventId}.");
        }

    }
}