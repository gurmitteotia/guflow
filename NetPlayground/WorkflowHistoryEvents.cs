using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private IEnumerable<HistoryEvent> _allHistoryEvents;

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
        }

        public WorkflowEvent GetActivityEvent(string activityName, string activityVersion, string positionalName = "")
        {
            throw new System.NotImplementedException();
        }
    }
}
