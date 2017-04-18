using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    internal class SchedulableWorkflowAction : WorkflowAction,  IFluentWorkflowActionItem
    {
        private readonly WorkflowAction _workflowAction;

        public SchedulableWorkflowAction(WorkflowAction workflowAction)
        {
            _workflowAction = workflowAction;
        }

        internal override IEnumerable<WorkflowDecision> GetDecisions()
        {
            throw new NotImplementedException();
        }

        public IFluentWorkflowActionItem After(string timerName)
        {
            throw new NotImplementedException();
        }

        public IFluentWorkflowActionItem After(string activityName, string activityVersion, string positionalName = "")
        {
            throw new NotImplementedException();
        }
    }
}