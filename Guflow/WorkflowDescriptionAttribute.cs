using System;
using System.Reflection;

namespace Guflow
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = true)]
    public class WorkflowDescriptionAttribute : Attribute
    {
        public WorkflowDescriptionAttribute(string version)
        {
            Version = version;
        }

        public string Name { get; set; }
        public string Version { get; private set; }
        public string Domain { get; set; }
        public string Description { get; set; }
        public string DefaultTaskListName { get; set; }
        public string DefaultChildPolicy { get; set; }
        public string DefaultLambdaRole { get; set; }
        public uint ExecutionStartToCloseTimeoutInSeconds { get; set; }
        public uint DefaultTaskStartToCloseTimeoutInSeconds { get; set; }

        internal static WorkflowDescriptionAttribute FindOn(Type workflowType)
        {
            return workflowType.GetCustomAttribute<WorkflowDescriptionAttribute>();
        }
    }
}