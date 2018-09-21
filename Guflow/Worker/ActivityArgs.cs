// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Worker
{
    /// <summary>
    /// Represent activity task argument.
    /// </summary>
    public class ActivityArgs
    {
        /// <summary>
        /// Create activity args.
        /// </summary>
        /// <param name="input">Activity input, optional </param>
        /// <param name="activityId">Activity id, required.</param>
        /// <param name="workflowId">Workflow id, required.</param>
        /// <param name="workflowRunId">Running workflow id, required.</param>
        /// <param name="taskToken">Task token, required.</param>
        public ActivityArgs(string input, string activityId, string workflowId, string workflowRunId, string taskToken)
        {
            Ensure.NotNullAndEmpty(activityId, nameof(activityId));
            Ensure.NotNullAndEmpty(workflowId, nameof(workflowId));
            Ensure.NotNullAndEmpty(workflowRunId, nameof(workflowRunId));
            Ensure.NotNullAndEmpty(taskToken, nameof(taskToken));

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
        /// Represents the activity task and used by Guflow when sending heartbeat or returning any response back to Amazon SWF.
        /// </summary>
        public string TaskToken { get; }
        /// <summary>
        /// Returns started event id in workflow history. You can use it for debugging purpose if needed.
        /// </summary>
        public long StartedEventId { get; internal set; }
    }
}