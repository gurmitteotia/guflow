namespace Guflow.Decider
{
    public sealed class JumpActions
    {
        private readonly WorkflowItems _workflowItems;
        private readonly WorkflowItem _triggeringWorkflowItem = null;
        internal JumpActions(WorkflowItems workflowItems)
        {
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