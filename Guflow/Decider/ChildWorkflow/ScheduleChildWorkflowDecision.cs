// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class ScheduleChildWorkflowDecision : WorkflowDecision
    {
        private readonly Identity _identity;
        private readonly object _input;

        public ScheduleChildWorkflowDecision(Identity identity, object input) : base(false)
        {
            _identity = identity;
            _input = input;
        }

        public string ChildPolicy { get; set; }
        public int? TaskPriority { get; set; }
        public string LambdaRole { get; set; }
        public string TaskListName { get; set; }
        public WorkflowTimeouts ExecutionTimeouts { get; set; }
        public string[] Tags { get; set; }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.StartChildWorkflowExecution,
                StartChildWorkflowExecutionDecisionAttributes = new StartChildWorkflowExecutionDecisionAttributes()
                {
                    WorkflowId = _identity.Id,
                    WorkflowType = new WorkflowType() { Name = _identity.Name, Version = _identity.Version},
                    Input = _input.ToAwsString(),
                    Control = new ScheduleData() { PN = _identity.PositionalName}.ToJson(),
                    ChildPolicy = ChildPolicy,
                    ExecutionStartToCloseTimeout = ExecutionTimeouts.ExecutionStartToCloseTimeout.Seconds(),
                    TaskStartToCloseTimeout = ExecutionTimeouts.TaskStartToCloseTimeout.Seconds(),
                    LambdaRole = LambdaRole,
                    TagList = Tags.ToList(),
                    TaskPriority = TaskPriority.SwfFormat(),
                    TaskList = TaskListName.TaskList()
                }
            };
        }

        public override bool Equals(object obj)
        {
            var decision = obj as ScheduleChildWorkflowDecision;
            return decision != null &&
                   EqualityComparer<Identity>.Default.Equals(_identity, decision._identity);
        }

        public override int GetHashCode()
        {
            return -1493283476 + EqualityComparer<Identity>.Default.GetHashCode(_identity);
        }
    }
}