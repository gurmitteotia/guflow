// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public abstract class ChildWorkflowEvent : WorkflowItemEvent
    {
        protected string WorkflowName;
        protected string WorkflowVersion;
        protected string PositionalName;
        private long _initiatedEventId;
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
        /// Returns the WorkflowId assigned to child workflow.
        /// </summary>
        public string WorkflowId { get; private set; }

        /// <summary>
        /// Internal method. Library user should not care about it.
        /// </summary>
        /// <param name="runid"></param>
        /// <param name="initiatedEventId"></param>
        /// <param name="allEvents"></param>
        protected void PopulateProperties(string runid, long initiatedEventId, IEnumerable<HistoryEvent> allEvents)
        {
            RunId = runid;
            _initiatedEventId = initiatedEventId;
            bool foundEvent = false;
            foreach (var historyEvent in allEvents)
            {
                if (historyEvent.IsChildWorkflowInitiatedEvent(initiatedEventId))
                {
                    var attr = historyEvent.StartChildWorkflowExecutionInitiatedEventAttributes;
                    Input = attr.Input;
                    WorkflowId = attr.WorkflowId;
                    ScheduleId = ScheduleId.Raw(attr.WorkflowId);
                    WorkflowName = attr.WorkflowType.Name;
                    WorkflowVersion = attr.WorkflowType.Version;
                    PositionalName = attr.Control.As<ScheduleData>().PN;
                    foundEvent = true;
                    break;
                }
            }

            if (!foundEvent)
                throw new IncompleteEventGraphException($"Can not find Child Workflow InitiatedEvent for id {initiatedEventId}");
        }

        public override string ToString()
        {
            return
                $"Event: {GetType().Name}, WorkflowName={WorkflowName}, Version={WorkflowVersion}, PositionalName={PositionalName}";
        }

        internal override bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            var childWorkflowEvents = workflowItemEvents.OfType<ChildWorkflowEvent>();
            foreach (var childWorkflowEvent in childWorkflowEvents)
            {
                if (IsInChain(childWorkflowEvent))
                    return true;
            }
            return false;
        }

        private bool IsInChain(ChildWorkflowEvent other)
        {
            return _initiatedEventId == other._initiatedEventId;
        }
    }
}