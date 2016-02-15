using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ScheduleActivityDecision : WorkflowDecision
    {
        private readonly WorkflowItemIdentity _identity;

        public ScheduleActivityDecision(string activityName, string activityVersion, string positionalName="")
        {
            _identity = new WorkflowItemIdentity(activityName,activityVersion,positionalName);
        }

        public override bool Equals(object other)
        {
            var otherDecision = other as ScheduleActivityDecision;
            if (otherDecision == null)
                return false;
            return _identity.Equals(otherDecision._identity);
        }

        public override IEnumerable<Decision> Decisions()
        {
            //return new[]
            //{
            //    new Decision()
            //    {
            //        ScheduleActivityTaskDecisionAttributes = new ScheduleActivityTaskDecisionAttributes()
            //        {
            //            ActivityType = new ActivityType() {Name = _activityName, Version = _activityVersion},
            //        },
            //        DecisionType = DecisionType.ScheduleActivityTask
            //    }
            //};
            throw new NotImplementedException();
       }
    }
}