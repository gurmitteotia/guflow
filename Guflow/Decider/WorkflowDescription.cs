// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using Guflow.Properties;
using Guflow.Worker;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents the description of workflow. It attributes are used while registering/interacting with Amazon SWF.
    /// </summary>
    public class WorkflowDescription
    {
        private static readonly IDescriptionStrategy Strategy = new CompositeDescriptionStrategy(new[] { DescriptionStrategy.FactoryMethod, DescriptionStrategy.FromAttribute });
        public WorkflowDescription(string version)
        {
            Version = version;
        }

        //Gets or sets the name of workflow.
        public string Name { get; set; }
        /// <summary>
        /// Gets the version of workflow.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets or sets the description of workflow.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the default task list name to poll for new decision task.
        /// </summary>
        public string DefaultTaskListName { get; set; }

        /// <summary>
        /// Gets or sets the timeout on how long workflow to stay active after it is started.
        /// </summary>
        public TimeSpan DefaultExecutionStartToCloseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for decision task to be processed by decider.
        /// </summary>
        public TimeSpan DefaultTaskStartToCloseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the child policy for this workflow. Child policy dictat how child workflow should be treated if its parent workflow is terminated.
        /// </summary>
        public string DefaultChildPolicy { get; set; }

        /// <summary>
        /// Gets or sets the default lambda role.
        /// </summary>
        public string DefaultLambdaRole { get; set; }
        /// <summary>
        /// Gets or sets the task priority.
        /// </summary>
        public int DefaultTaskPriority { get; set; }

        internal static WorkflowDescription FindOn<T>() where T: Workflow
        {
            return FindOn(typeof(T));
        }
        internal static WorkflowDescription FindOn(Type workflowType)
        {
            Ensure.NotNull(workflowType, "workflowType");
            if (!typeof(Workflow).IsAssignableFrom(workflowType))
                throw new NonWorkflowTypeException(string.Format(Resources.Non_Workflow_type, workflowType.Name, typeof(Workflow).Name));
            var workflowDescription = Strategy.FindDescription(workflowType);

            if (workflowDescription == null)
                throw new WorkflowDescriptionMissingException(string.Format(Resources.Workflow_description_not_supplied, workflowType.Name));

            return workflowDescription;
        }
    }
}