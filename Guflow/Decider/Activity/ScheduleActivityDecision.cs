// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly Identity _identity;
        private Func<string> _inputFunc;
        public ScheduleActivityDecision(Identity identity) : base(false)
        {
            _identity = identity;
            _inputFunc = () => null;
        }

        public ActivityTimeouts Timeouts { get; internal set; }
        public string Input => _inputFunc();
        public string TaskList { get; set; }
        public int? TaskPriority { get; set; }


        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_identity);
        }

        internal void UseInputFunc(Func<string> inputFunc)
        {
            _inputFunc = inputFunc;
        }
        public override bool Equals(object other)
        {
            var otherDecision = other as ScheduleActivityDecision;
            if (otherDecision == null)
                return false;
            return _identity.Equals(otherDecision._identity);
        }

        public override int GetHashCode()
        {
            return _identity.GetHashCode();
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                {
                    ActivityType = new ActivityType() { Name = _identity.Name, Version = _identity.Version },
                    ActivityId = _identity.Id,
                    Control = (new ActivityScheduleData() { PN = _identity.PositionalName}).ToJson(),
                    HeartbeatTimeout = ToAwsTimeout(Timeouts.HeartbeatTimeout),
                    ScheduleToCloseTimeout = ToAwsTimeout(Timeouts.ScheduleToCloseTimeout),
                    ScheduleToStartTimeout = ToAwsTimeout(Timeouts.ScheduleToStartTimeout),
                    StartToCloseTimeout = ToAwsTimeout(Timeouts.StartToCloseTimeout),
                    Input = Input,
                    TaskList = ToAwsTaskList(TaskList),
                    TaskPriority = ToAwsTaskPriority(TaskPriority)
                },
                DecisionType = DecisionType.ScheduleActivityTask
            };
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _identity);
        }

        private string ToAwsTaskPriority(int? taskPriority)
        {
            if (!taskPriority.HasValue)
                return null;
            return taskPriority.Value.ToString();
        }

        private Amazon.SimpleWorkflow.Model.TaskList ToAwsTaskList(string taskList)
        {
            if (string.IsNullOrEmpty(taskList))
                return null;
            return new Amazon.SimpleWorkflow.Model.TaskList() {Name = taskList};
        }

        private string ToAwsTimeout(TimeSpan? timeout)
        {
            if (!timeout.HasValue)
                return null;
            if (timeout.Value == TimeSpan.MaxValue)
                return "NONE";
            return timeout.Value.TotalSeconds.ToString();
        }
    }
}