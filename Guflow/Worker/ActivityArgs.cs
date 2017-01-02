using Amazon.SimpleWorkflow;

namespace Guflow.Worker
{
    public class ActivityArgs
    {
        internal ActivityArgs(string input, string activityId, string workflowId, string workflowRunId, string taskToken)
        {
            Input = input;
            ActivityId = activityId;
            WorkflowId = workflowId;
            WorkflowRunId = workflowRunId;
            TaskToken = taskToken;
            IAmazonSimpleWorkflow sw;
        }

        public string ActivityId { get; private set; }
        public string Input { get; private set; }
        public string WorkflowId { get; private set; }
        public string WorkflowRunId { get; private set; }
        public string TaskToken { get; private set; }
        public long StartedEventId { get; internal set; }
    }
}