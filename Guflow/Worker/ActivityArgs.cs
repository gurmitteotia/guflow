// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Worker
{
    /// <summary>
    /// Represent activity task argument.
    /// </summary>
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
        /// <summary>
        /// Unique Id of activity.
        /// </summary>
        public string ActivityId { get; }

        /// <summary>
        /// Activity input supplied by scheduling workflow.
        /// </summary>
        public string Input { get; }
        /// <summary>
        /// Workflow id of scheduling workflow.
        /// </summary>
        public string WorkflowId { get; }
        /// <summary>
        /// Workflow run id of scheduling workflow.
        /// </summary>
        public string WorkflowRunId { get; }
        /// <summary>
        /// Represents the activity task and used by Guflow when sending hearbeat or returning any reponse back to Amazon SWF.
        /// </summary>
        public string TaskToken { get; }
        /// <summary>
        /// Returns started event id in workflow history. You can use it for debugging purpose if needed.
        /// </summary>
        public long StartedEventId { get; internal set; }
    }
}