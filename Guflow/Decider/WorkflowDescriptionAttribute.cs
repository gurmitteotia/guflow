// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Configuration;
using System.Reflection;
using Amazon.SimpleWorkflow.Model;
using Guflow.Properties;

namespace Guflow.Decider
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class WorkflowDescriptionAttribute : Attribute
    {
       
        public WorkflowDescriptionAttribute(string version)
        {
           Version = version;
        }

        /// <summary>
        /// Gets or sets the workflow name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the workflow version.
        /// </summary>
        public string Version { get; }
        /// <summary>
        /// Gets the workflow description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the default task list name
        /// </summary>
        public string DefaultTaskListName { get; set; }

        /// <summary>
        /// Gets or sets the default child policy. Child policy determine how child workflow is treated when its parent workflow is terminated.
        /// </summary>
        public string DefaultChildPolicy { get; set; }

        /// <summary>
        /// Gets or sets the default lambada role.
        /// </summary>
        public string DefaultLambdaRole { get; set; }

        /// <summary>
        /// Gets or sets the workflow start to close timeout in seconds.
        /// </summary>
        public uint DefaultExecutionStartToCloseTimeoutInSeconds { get; set; }
      
        /// <summary>
        /// Gets or sets the timout for decision tasks.
        /// </summary>
        public uint DefaultTaskStartToCloseTimeoutInSeconds { get; set; }
        
        /// <summary>
        /// Gets or sets the default task priority.
        /// </summary>
        public int DefaultTaskPriority { get; set; }
        internal WorkflowDescription WorkflowDescription()
        {
            return new WorkflowDescription(Version)
            {
                DefaultTaskListName = DefaultTaskListName,
                Name = Name,
                Description = Description,
                DefaultTaskPriority = DefaultTaskPriority,
                DefaultChildPolicy = DefaultChildPolicy,
                DefaultLambdaRole = DefaultLambdaRole,
                DefaultTaskStartToCloseTimeout = AwsFormat(DefaultTaskStartToCloseTimeoutInSeconds),
                DefaultExecutionStartToCloseTimeout = AwsFormat(DefaultExecutionStartToCloseTimeoutInSeconds)
            };
        }

        private TimeSpan? AwsFormat(uint seconds)
        {
            if (seconds == 0) return null;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}