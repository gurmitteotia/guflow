using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class WorkflowHistoryEvents : IWorkflowHistoryEvents
    {
        private IEnumerable<HistoryEvent> _allHistoryEvents;

        public WorkflowHistoryEvents(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
        }

        public ActivityEvent GetActivityEvent(string activityName, string activityVersion, string positionalName = "")
        {
            throw new System.NotImplementedException();
        }
    }
}
