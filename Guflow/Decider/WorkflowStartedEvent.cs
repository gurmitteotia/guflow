// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Raised when workflow is started.
    /// </summary>
    public class WorkflowStartedEvent : WorkflowEvent
    {
        private readonly WorkflowExecutionStartedEventAttributes _workflowStartedAttributes;

        internal WorkflowStartedEvent(HistoryEvent workflowStartedEvent)
            : base(workflowStartedEvent.EventId)
        {
            _workflowStartedAttributes = workflowStartedEvent.WorkflowExecutionStartedEventAttributes;
        }

        /// <summary>
        /// Returns the child policy.  Child policy determine the fate of child workflow when parent workflow is terminated. 
        /// </summary>
        public string ChildPolicy => _workflowStartedAttributes.ChildPolicy == null ? string.Empty : _workflowStartedAttributes.ChildPolicy.Value;

        internal override WorkflowAction DefaultAction(IWorkflowDefaultActions defaultActions)
        {
            return defaultActions.StartWorkflow();
        }
        /// <summary>
        /// Run ID of the previous workflow, if this workflow was started as result of <see cref="RestartWorkflowAction"/>
        /// </summary>
        public string ContinuedExecutionRunId => _workflowStartedAttributes.ContinuedExecutionRunId;
        /// <summary>
        /// Returns the maximum duration this workflow should complete its execution.
        /// </summary>
        public TimeSpan ExecutionStartToCloseTimeout => TimeSpan.FromSeconds(Convert.ToInt32(_workflowStartedAttributes.ExecutionStartToCloseTimeout));
        /// <summary>
        /// Returns the workflow input in raw form
        /// </summary>
        public string Input => _workflowStartedAttributes.Input;
        /// <summary>
        /// Returns the lambda role this workflow is assigned to. Lambda role is needed by workflow to invoke the lambda functions.
        /// </summary>
        public string LambdaRole => _workflowStartedAttributes.LambdaRole;

        /// <summary>
        /// 
        /// </summary>
        public long ParentInitiatedEventId => _workflowStartedAttributes.ParentInitiatedEventId;

        /// <summary>
        /// Returns parent workflow run id if this workflow was started as child workflow otherwise empty string is returned.
        /// </summary>
        public string ParentWorkflowRunId
        {
            get
            {
                if (_workflowStartedAttributes.ParentWorkflowExecution != null)
                    return _workflowStartedAttributes.ParentWorkflowExecution.RunId;
                return string.Empty;
            }
        }
        /// <summary>
        /// Returns parent workflow id if this workflow was started as child workflow otherwise empty string is returned.
        /// </summary>
        public string ParentWorkflowId
        {
            get
            {
                if (_workflowStartedAttributes.ParentWorkflowExecution != null)
                    return _workflowStartedAttributes.ParentWorkflowExecution.WorkflowId;
                return string.Empty;
            }
        }
        /// <summary>
        /// Returns the tags assigned to this workflow.
        /// </summary>
        public IEnumerable<string> TagList => _workflowStartedAttributes.TagList;
        /// <summary>
        /// Returns the task list this workflow is started on.
        /// </summary>
        public string TaskList => _workflowStartedAttributes.TaskList.Name;
        /// <summary>
        /// Returns the priority this workflow is started with in Amazon SWF.
        /// </summary>
        public int? TaskPriority
        {
            get
            {
                if (string.IsNullOrEmpty(_workflowStartedAttributes.TaskPriority))
                    return null;

                return int.Parse(_workflowStartedAttributes.TaskPriority);
            }
        }

        /// <summary>
        /// Returns the maximum duration of decision tasks for this workflow. In other words workflow should return its decisions to Amazon SWF during this timeout.
        /// </summary>
        public TimeSpan TaskStartToCloseTimeout => TimeSpan.FromSeconds(Convert.ToInt32(_workflowStartedAttributes.TaskStartToCloseTimeout));

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.WorkflowAction(this);
        }
    }
}
