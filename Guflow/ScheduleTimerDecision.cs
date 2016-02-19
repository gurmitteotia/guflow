using System;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class ScheduleTimerDecision : WorkflowDecision
    {
        private readonly string _timerName;
        private readonly TimeSpan _fireAfter;

        public ScheduleTimerDecision(string timerName, TimeSpan fireAfter= new TimeSpan())
        {
            _timerName = timerName;
            _fireAfter = fireAfter;
        }

        public override bool Equals(object other)
        {
            var otherTimer = other as ScheduleTimerDecision;
            if (otherTimer == null)
                return false;
            return string.Equals(_timerName, otherTimer._timerName);
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(_timerName) ? GetType().GetHashCode() : _timerName.GetHashCode();
        }

        public override Decision Decision()
        {
            throw new NotImplementedException();
        }
    }
}