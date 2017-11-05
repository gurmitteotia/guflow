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
        }

        public string ActivityId { get; }
        public string Input { get; }
        public string WorkflowId { get; }
        public string WorkflowRunId { get; }
        public string TaskToken { get; }
        public long StartedEventId { get; internal set; }
    }
}