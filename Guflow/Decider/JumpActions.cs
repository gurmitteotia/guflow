namespace Guflow.Decider
{
    public sealed class JumpActions
    {
        private readonly WorkflowItem _triggeringWorkflowItem;
        private readonly WorkflowItems _workflowItems;

        internal JumpActions(WorkflowItem triggeringWorkflowItem, WorkflowItems workflowItems)
        {
            _triggeringWorkflowItem = triggeringWorkflowItem;
            _workflowItems = workflowItems;
        }
        

        public JumpWorkflowAction ToActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItemFor(Identity.New(name, version, positionalName));
            return WorkflowAction.JumpTo(activityItem);
        }
        public JumpWorkflowAction ToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");

            var activityItem = _workflowItems.TimerItemFor(Identity.Timer(name));
            return WorkflowAction.JumpTo(activityItem);
        }
    }
}