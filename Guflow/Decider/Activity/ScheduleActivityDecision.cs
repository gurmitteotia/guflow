// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly Identity _identity;
        public ScheduleActivityDecision(Identity identity) : base(false)
        {
            _identity = identity;
        }

        public ActivityTimeouts Timeouts { get; internal set; }
        public string Input { get; set; }
   
        public string TaskListName { get; set; }
        public int? TaskPriority { get; set; }


        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_identity);
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
                    Control = (new ScheduleData() { PN = _identity.PositionalName}).ToJson(),
                    HeartbeatTimeout =Timeouts.HeartbeatTimeout.Seconds(),
                    ScheduleToCloseTimeout = Timeouts.ScheduleToCloseTimeout.Seconds(),
                    ScheduleToStartTimeout = Timeouts.ScheduleToStartTimeout.Seconds(),
                    StartToCloseTimeout = Timeouts.StartToCloseTimeout.Seconds(),
                    Input = Input,
                    TaskList = TaskListName.TaskList(),
                    TaskPriority = TaskPriority.SwfFormat()
                },
                DecisionType = DecisionType.ScheduleActivityTask
            };
        }

        public override string ToString()
        {
            return string.Format("{0} for {1}", GetType().Name, _identity);
        }
    }
}