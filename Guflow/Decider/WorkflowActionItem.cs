using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowActionItem : WorkflowItem, IFluentWorkflowActionItem
    {
        private readonly WorkflowAction _workflowAction;
        public WorkflowActionItem(WorkflowAction workflowAction, IWorkflow workflow) : base(RandomIdentity(), workflow)
        {
            _workflowAction = workflowAction;
        }

        public override WorkflowItemEvent LastEvent
        {
            get { return WorkflowItemEvent.NotFound; }
        }

        public override WorkflowDecision GetScheduleDecision()
        {
            return WorkflowDecision.Empty;
        }

        public override WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout)
        {
            return WorkflowDecision.Empty;
        }

        public override WorkflowDecision GetCancelDecision()
        {
            return WorkflowDecision.Empty;
        }

        public override IEnumerable<WorkflowDecision> GetContinuedDecisions()
        {
            return _workflowAction.GetDecisions();
        }

        private static Identity RandomIdentity()
        {
            return Identity.New(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public IFluentWorkflowActionItem After(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }

        public IFluentWorkflowActionItem After(string activityName, string activityVersion, string positionalName = "")
        {
            AddParent(Identity.New(activityName, activityVersion, positionalName));
            return this;
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { return Enumerable.Empty<WorkflowItemEvent>(); }
        }
    }
}