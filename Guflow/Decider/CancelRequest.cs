using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    public class CancelRequest
    {
        private readonly WorkflowItems _workflowItems;
        internal CancelRequest(WorkflowItems workflowItems)
        {
            _workflowItems = workflowItems;
        }
        public  WorkflowAction ForActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name,"name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItemFor(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(activityItem);
        }
        public WorkflowAction ForTimer(string timerName)
        {
            Ensure.NotNullAndEmpty(timerName, "timerName");

            var timerItem = _workflowItems.TimerItemFor(Identity.Timer(timerName));
            return WorkflowAction.Cancel(timerItem);
        }
        public WorkflowAction ForWorkflow(string workflowId, string runId = null)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            return WorkflowAction.CancelWorkflowRequest(workflowId, runId);
        }

        public WorkflowAction For(IEnumerable<IWorkflowItem> workflowItems)
        {
            Ensure.NotNull(workflowItems, "workflowItems");
            return WorkflowAction.Cancel(workflowItems.OfType<WorkflowItem>());
        }
    }
}