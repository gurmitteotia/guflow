// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public abstract class ChildWorkflowEvent : WorkflowItemEvent
    {
        protected ChildWorkflowEvent(long eventId) : base(eventId)
        {
        }
        /// <summary>
        /// Returns the input passed to child workflow when scheduling it.
        /// </summary>
        public string Input { get; private set; }

        /// <summary>
        /// Returns the RunId assigned by AmazonSWF to start this child workflow.
        /// </summary>
        public string RunId { get; private set; }

        /// <summary>
        /// Internal method. Library user should not care about it.
        /// </summary>
        /// <param name="runid"></param>
        /// <param name="initiatedEventId"></param>
        /// <param name="allEvents"></param>
        protected void PopulateProperties(string runid, long initiatedEventId,
            IEnumerable<HistoryEvent> allEvents)
        {
            RunId = runid;
            bool foundEvent = false;
            foreach (var historyEvent in allEvents)
            {
                if (historyEvent.IsChildWorkflowInitiatedEvent(initiatedEventId))
                {
                    var attr = historyEvent.StartChildWorkflowExecutionInitiatedEventAttributes;
                    Input = attr.Input;
                    AwsIdentity = AwsIdentity.Raw(attr.WorkflowId);
                    foundEvent = true;
                    break;
                }
            }

            if (!foundEvent)
                throw new IncompleteEventGraphException($"Can not find Child Workflow InitiatedEvent for id {initiatedEventId}");
        }
    }
}