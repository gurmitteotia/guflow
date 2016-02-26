using System;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class ScheduleTimerDecision : WorkflowDecision
    {
        private readonly Identity _timerIdentity;
        private readonly TimeSpan _fireAfter;

        public ScheduleTimerDecision(Identity timerIdentity, TimeSpan fireAfter)
        {
            _timerIdentity = timerIdentity;
            _fireAfter = fireAfter;
        }

        public override bool Equals(object other)
        {
            var otherTimer = other as ScheduleTimerDecision;
            if (otherTimer == null)
                return false;
            return string.Equals(_timerIdentity, otherTimer._timerIdentity) && _fireAfter.Equals(otherTimer._fireAfter);
        }

        public override int GetHashCode()
        {
            return _timerIdentity.GetHashCode();
        }

        public override Decision Decision()
        {
            throw new NotImplementedException();
        }
    }
}