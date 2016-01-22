using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityCompletedEvent : WorkflowEvent
    {
        public ActivityCompletedEvent(HistoryEvent activityCompletedEvent)
        {
            Result = activityCompletedEvent.ActivityTaskCompletedEventAttributes.Result;
            PopulateEvent(activityCompletedEvent.ActivityTaskCompletedEventAttributes);
        }

        private void PopulateEvent(ActivityTaskCompletedEventAttributes activityTaskCompletedEventAttributes)
        {
        }

        public string Result { get; private set; }

        public ActivityName ActivityName { get; set; }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.ActivityCompleted(this);            
        }
    }
}