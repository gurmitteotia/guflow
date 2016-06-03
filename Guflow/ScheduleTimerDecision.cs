using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    internal class ScheduleTimerDecision : WorkflowDecision
    {
        private readonly Identity _timerIdentity;
        private readonly TimeSpan _fireAfter;
        private readonly bool _isRescheduleTimer;
        public ScheduleTimerDecision(Identity timerIdentity, TimeSpan fireAfter, bool isRescheduleTimer=false)
        {
            _timerIdentity = timerIdentity;
            _fireAfter = fireAfter;
            _isRescheduleTimer = isRescheduleTimer;
        }

        public override bool Equals(object other)
        {
            var otherTimer = other as ScheduleTimerDecision;
            if (otherTimer == null)
                return false;
            return string.Equals(_timerIdentity, otherTimer._timerIdentity) && _fireAfter.Equals(otherTimer._fireAfter)
                   && _isRescheduleTimer == otherTimer._isRescheduleTimer;
        }

        public override int GetHashCode()
        {
            return _timerIdentity.GetHashCode();
        }

        public override Decision Decision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.StartTimer,
                StartTimerDecisionAttributes = new StartTimerDecisionAttributes()
                {
                    TimerId = _timerIdentity.Id.ToString(),
                    StartToFireTimeout = Math.Round(_fireAfter.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() {IsARescheduleTimer = _isRescheduleTimer, TimerName = _timerIdentity.Name}).ToJson()
                }
            };
        }
    }
}