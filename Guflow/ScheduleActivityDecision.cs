using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly Identity _identity;
        internal ScheduleActivityDecision(Identity identity)
        {
            _identity = identity;
        }

        public TimeSpan? HeartbeatTimeout { get; set; }
        public TimeSpan? ScheduleToCloseTimeout { get; set; }
        public TimeSpan? ScheduleToStartTimeout { get; set; }
        public TimeSpan? StartToCloseTimeout { get; set; }
        public string Input { get; set; }
        public string TaskList { get; set; }
        public int? TaskPriority { get; set; }

        public override bool Equals(object other)
        {
            var otherDecision = other as ScheduleActivityDecision;
            if (otherDecision == null)
                return false;
            return _identity.Equals(otherDecision._identity);
        }

        public override Decision Decision()
        {
            return new Decision()
            {
                ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                {
                    ActivityType = new ActivityType() { Name = _identity.Name, Version = _identity.Version },
                    ActivityId = _identity.Id,
                    Control = (new ActivityScheduleData() { PN = _identity.PositionalName}).ToJson(),
                    HeartbeatTimeout = ToAwsTimeout(HeartbeatTimeout),
                    ScheduleToCloseTimeout = ToAwsTimeout(ScheduleToCloseTimeout),
                    ScheduleToStartTimeout = ToAwsTimeout(ScheduleToStartTimeout),
                    StartToCloseTimeout = ToAwsTimeout(StartToCloseTimeout),
                    Input = Input,
                    TaskList = ToAwsTaskList(TaskList),
                    TaskPriority = ToAwsTaskPriority(TaskPriority)
                },
                DecisionType = DecisionType.ScheduleActivityTask
            };
        }

        public override string ToString()
        {
            return _identity.ToString();
        }

        private string ToAwsTaskPriority(int? taskPriority)
        {
            if (!taskPriority.HasValue)
                return null;
            return taskPriority.Value.ToString();
        }

        private TaskList ToAwsTaskList(string taskList)
        {
            if (string.IsNullOrEmpty(taskList))
                return null;
            return new TaskList() {Name = taskList};
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