// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents the child workflow completed event 
    /// </summary>
    public class ChildWorkflowCompletedEvent : WorkflowItemEvent
    {
        private ChildWorkflowExecutionCompletedEventAttributes _attr;
        internal ChildWorkflowCompletedEvent(HistoryEvent completedEvent, IEnumerable<HistoryEvent> allEvents) : base(completedEvent.EventId)
        {
            _attr = completedEvent.ChildWorkflowExecutionCompletedEventAttributes;
            PopulateProperties(_attr.InitiatedEventId, allEvents);
        }

        /// <summary>
        /// Returns completed result of child workflow.
        /// </summary>
        public string Result => _attr.Result;

        /// <summary>
        /// Returns the input passed to child workflow when scheduling it.
        /// </summary>
        public string Input { get; private set; }
        /// <summary>
        /// Returns the RunId assigned by AmazonSWF to start this child workflow.
        /// </summary>
        public string RunId => _attr.WorkflowExecution.RunId;

        private void PopulateProperties(long initiatedEventId, IEnumerable<HistoryEvent> allEvents)
        {
            bool foundEvent = false;
            foreach (var historyEvent in allEvents)
            {
                if (historyEvent.IsChildWorkflowInitiatedEvent(initiatedEventId))
                {
                    var attr = historyEvent.StartChildWorkflowExecutionInitiatedEventAttributes;
                    Input = attr.Input;
                    foundEvent = true;
                    break;
                }
            }

            if(!foundEvent)
                throw new IncompleteEventGraphException($"Can not find Child Workflow InitiatedEvent for id {initiatedEventId}");
        }
    }
}