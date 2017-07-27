using System;

namespace Guflow.Decider
{
    public sealed class JumpAction
    {
        private readonly WorkflowItems _workflowItems;
        private readonly Func<WorkflowItem, WorkflowAction> _triggeringAction;
        private JumpAction(WorkflowItems workflowItems, Func<WorkflowItem, WorkflowAction> triggeringAction)
        {
            _workflowItems = workflowItems;
            _triggeringAction = triggeringAction;
        }

        internal static JumpAction JumpFromItem(WorkflowItem jumpFromItem, WorkflowItems workflowItems)
        {
            return new JumpAction(workflowItems, (to)=>new JumpTriggeredWorkflowAction(jumpFromItem, to));
        }

        internal static JumpAction JumpFromNonItem(WorkflowItems workflowItems)
        {
            return new JumpAction(workflowItems, (to) => WorkflowAction.Empty);
        }

        public JumpWorkflowAction ToActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItemFor(Identity.New(name, version, positionalName));
            return WorkflowAction.JumpTo(activityItem).SetTriggerAction(_triggeringAction(activityItem));
        }
        public JumpWorkflowAction ToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");
            var timerItem = _workflowItems.TimerItemFor(Identity.Timer(name));
            return WorkflowAction.JumpTo(timerItem).SetTriggerAction(_triggeringAction(timerItem));
        }
    }
}