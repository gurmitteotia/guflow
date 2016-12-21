﻿using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class StartWorkflowRequest
    {
        public StartWorkflowRequest(string workflowName, string version, string workflowId)
        {
            Ensure.NotNullAndEmpty(workflowName, "workflowName");
            Ensure.NotNullAndEmpty(version, "version");
            Ensure.NotNullAndEmpty(workflowId, "workflowId");

            WorkflowName = workflowName;
            Version = version;
            WorkflowId = workflowId;
        }

        public string WorkflowName { get; private set; }
        public string Version { get; private set; }
        public string WorkflowId { get; private set; }
        public string Input { get; set; }
        public string ChildPolicy { get; set; }
        public string LambdaRole { get; set; }
        public string TaskListName { get; set; }
        public int? TaskPriority { get; set; }
        public List<string> Tags { get; set; }
        public TimeSpan? TaskStartToCloseTimeout { get; set; }
        public TimeSpan? ExecutionStartToCloseTimeout { get; set; }

        public static StartWorkflowRequest For<T>(string workflowId) where T: Workflow
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            var workflowDescription = WorkflowDescriptionAttribute.FindOn<T>();
            return new StartWorkflowRequest(workflowDescription.Name, workflowDescription.Version, workflowId);
        }

        internal StartWorkflowExecutionRequest SwfFormat(string domainName)
        {
            return new StartWorkflowExecutionRequest
            {
                WorkflowType = new WorkflowType {Name = WorkflowName, Version = Version},
                Domain = domainName,
                TaskList = TaskListName.TaskList(),
                WorkflowId = WorkflowId,
                Input = Input,
                ChildPolicy = ChildPolicy,
                LambdaRole = LambdaRole,
                TagList = Tags,
                TaskPriority = TaskPriority.SwfFormat(),
                TaskStartToCloseTimeout = TaskStartToCloseTimeout.SwfFormat(),
                ExecutionStartToCloseTimeout = ExecutionStartToCloseTimeout.SwfFormat()

            };
        }
    }
}