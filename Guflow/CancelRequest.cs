namespace Guflow
{
    public class CancelRequest
    {
        private readonly IWorkflowItems _workflowItems;
        internal CancelRequest(IWorkflowItems workflowItems)
        {
            _workflowItems = workflowItems;
        }
        public  WorkflowAction ForActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name,"name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.FindActivityFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(activityItem);
        }
        public WorkflowAction ForTimer(string timerName)
        {
            Ensure.NotNullAndEmpty(timerName, "timerName");

            var timerItem = _workflowItems.FindTimerFor(Identity.Timer(timerName));
            return WorkflowAction.Cancel(timerItem);
        }
        public WorkflowAction ForWorkflow(string workflowId, string runId = null)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            return WorkflowAction.CancelWorkflowRequest(workflowId, runId);
        }
    }
}