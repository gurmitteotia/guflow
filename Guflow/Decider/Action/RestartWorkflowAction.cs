// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Cause the workflow to restart. By default it restart the workflow with same properties as workflow was started.
    /// </summary>
    public class RestartWorkflowAction : WorkflowAction
    {
        private readonly List<string> _tags = new List<string>();
        /// <summary>
        /// Gets or sets the task priority for restarted workflow.
        /// </summary>
        public int? TaskPriority { get;set; }
        /// <summary>
        /// Gets the tag list for restarted workflow.
        /// </summary>
        public IEnumerable<string> TagList => _tags;

        /// <summary>
        /// Gets or sets the child policiy for restarted workflow.
        /// </summary>
        public string ChildPolicy { get; set; }

        /// <summary>
        /// Gets or sets the input for restarted workflow.
        /// </summary>
        public string Input { get; set; }
        /// <summary>
        /// Gets or sets the execution start to close timout for restarted workflow.
        /// </summary>
        public TimeSpan? ExecutionStartToCloseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the tasklist.
        /// </summary>
        public string TaskList { get; set; }

        /// <summary>
        /// Gets or sets the decision task timeout.
        /// </summary>
        public TimeSpan? TaskStartToCloseTimeout { get; set; }
        /// <summary>
        /// Get or sets the workflow version.
        /// </summary>
        public string WorkflowTypeVersion { get; set; }

        /// <summary>
        /// Gets or sets the defaul lambda role.
        /// </summary>
        public string DefaultLambdaRole { get; set; }

        /// <summary>
        /// Associate a tag with restarted workflow.
        /// </summary>
        /// <param name="tag"></param>
        public void AddTag(string tag)
        {
            _tags.Add(tag);
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return new[] {new RestartWorkflowDecision(this), };
        }
    }
}