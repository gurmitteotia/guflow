// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly ScheduleId _id;
        public ScheduleActivityDecision(ScheduleId id) : base(false)
        {
            _id = id;
        }

        public ActivityTimeouts Timeouts { get; internal set; }
        public string Input { get; set; }
   
        public string TaskListName { get; set; }
        public int? TaskPriority { get; set; }


        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_id);
        }
        public override bool Equals(object other)
        {
            var otherDecision = other as ScheduleActivityDecision;
            if (otherDecision == null)
                return false;
            return _id.Equals(otherDecision._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                {
                    ActivityType = new ActivityType() { Name = _id.Name, Version = _id.Version },
                    ActivityId = _id,
                    Control = (new ScheduleData() { PN = _id.PositionalName}).ToJson(),
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
            return string.Format("{0} for {1}", GetType().Name, _id);
        }
    }
}