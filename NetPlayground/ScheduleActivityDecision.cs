using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly string _activityName;
        private readonly string _activityVersion;
        private string _positionalName;

        public ScheduleActivityDecision(string activityName, string activityVersion, string positionalName)
        {
            _activityName = activityName;
            _activityVersion = activityVersion;
            _positionalName = positionalName;
        }


        public override IEnumerable<Decision> Decisions()
        {
            return new[]
            {
                new Decision()
                {
                    ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
                    {
                        ActivityType = new ActivityType() {Name = _activityName, Version = _activityVersion},
                    },
                    DecisionType = DecisionType.ScheduleActivityTask
                }
            };
    }
    }
}