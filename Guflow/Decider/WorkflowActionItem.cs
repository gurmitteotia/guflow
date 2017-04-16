using System.Collections.Generic;

namespace Guflow.Decider
{
    internal class WorkflowActionItem : WorkflowItem, IFluentWorkflowActionItem
    {
        private readonly WorkflowAction _workflowAction;

        public WorkflowActionItem(WorkflowAction workflowAction): base(Identity.New("fdf", "ver"),null)
        {
            _workflowAction = workflowAction;
        }

        public IFluentWorkflowActionItem After(string timerName)
        {
            throw new System.NotImplementedException();
        }

        public IFluentWorkflowActionItem After(string activityName, string activityVersion, string positionalName = "")
        {
            throw new System.NotImplementedException();
        }

        public override WorkflowItemEvent LastEvent
        {
            get { throw new System.NotImplementedException(); }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}