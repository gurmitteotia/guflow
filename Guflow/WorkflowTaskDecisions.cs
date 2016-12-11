using System.Collections.Generic;

namespace Guflow
{
    internal class WorkflowTaskDecisions
    {
        private IEnumerable<WorkflowDecision> _workflowDecisions;
        private IPostExecutionEvents _postExecutionEvents;
    }
}