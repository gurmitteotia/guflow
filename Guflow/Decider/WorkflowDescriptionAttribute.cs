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
        private uint? _defaultExecutionStartToCloseTimeout;
        private uint? _defaultTaskStartToCloseTimeout;

        public WorkflowDescriptionAttribute(string version)
        {
           Version = version;
        }

        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string DefaultTaskListName { get; set; }
        public string DefaultChildPolicy { get; set; }
        public string DefaultLambdaRole { get; set; }

        public uint DefaultExecutionStartToCloseTimeoutInSeconds
        {
            get
            {
                return _defaultExecutionStartToCloseTimeout ?? default(uint);
            }
            set { _defaultExecutionStartToCloseTimeout = value; }
        }

        public uint DefaultTaskStartToCloseTimeoutInSeconds
        {
            get
            {
                return _defaultTaskStartToCloseTimeout ?? default(uint);
            }
            set { _defaultTaskStartToCloseTimeout = value; }
        }
        public int DefaultTaskPriority { get; set; }

        internal string DefaultExecutionStartToCloseTimeout
        {
            get
            {
                return _defaultExecutionStartToCloseTimeout.SwfFormat();
            }
        }

        internal string DefaultTaskStartToCloseTimeout { get { return _defaultTaskStartToCloseTimeout.SwfFormat(); } }

        public static WorkflowDescriptionAttribute FindOn<TWorkflow>() where TWorkflow : Workflow
        {
            return FindOn(typeof(TWorkflow));
        }
        public static WorkflowDescriptionAttribute FindOn(Type workflowType)
        {
            Ensure.NotNull(workflowType, "workflowType");
            if(!typeof(Workflow).IsAssignableFrom(workflowType))
                throw new NonWorkflowTypeException(string.Format(Resources.Non_Workflow_type,workflowType.Name,typeof(Workflow).Name));

            var workflowDescription = workflowType.GetCustomAttribute<WorkflowDescriptionAttribute>();
            if (workflowDescription == null)
                throw new WorkflowDescriptionMissingException(string.Format(Resources.Workflow_attribute_missing, workflowType.Name));
            
            if (string.IsNullOrWhiteSpace(workflowDescription.Version))
                throw new ConfigurationErrorsException(string.Format(Resources.Empty_version, workflowType.Name));

            if (string.IsNullOrWhiteSpace(workflowDescription.Name))
                workflowDescription.Name = workflowType.Name;
            return workflowDescription;
        }

        internal RegisterWorkflowTypeRequest RegisterRequest(string domainName)
        {
            return new RegisterWorkflowTypeRequest
            {
                Name = Name,
                Version = Version,
                Description = Description,
                Domain = domainName,
                DefaultExecutionStartToCloseTimeout = DefaultExecutionStartToCloseTimeout,
                DefaultTaskList = DefaultTaskListName.TaskList(),
                DefaultTaskStartToCloseTimeout = DefaultTaskStartToCloseTimeout,
                DefaultChildPolicy = DefaultChildPolicy,
                DefaultLambdaRole = DefaultLambdaRole,
                DefaultTaskPriority = DefaultTaskPriority.ToString()
            };
        }
    }
}