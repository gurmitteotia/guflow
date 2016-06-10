using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ActivityScheduledEvent : ActivityEvent
    {
        public ActivityScheduledEvent(HistoryEvent scheduledActivityEvent)
        {
            PopulateActivityFrom();
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new System.NotImplementedException();
        }
    }
}