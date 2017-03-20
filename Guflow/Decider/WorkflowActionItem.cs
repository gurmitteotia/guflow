namespace Guflow.Decider
{
    internal class WorkflowActionItem :  IFluentWorkflowActionItem
    {
        private readonly WorkflowAction _workflowAction;

        public WorkflowActionItem(WorkflowAction workflowAction)
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
    }
}