using System;
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

        public override bool Equals(object other)
        {
            var otherDecision = other as ScheduleActivityDecision;
            if (otherDecision == null)
                return false;
            return _identity.Equals(otherDecision._identity);
        }

        public override Decision Decision()
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