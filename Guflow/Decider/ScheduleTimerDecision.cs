// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class ScheduleTimerDecision : WorkflowDecision
    {
        private readonly Identity _timerIdentity;
        private readonly TimeSpan _fireAfter;
        private readonly bool _isRescheduleTimer;
        public ScheduleTimerDecision(Identity timerIdentity, TimeSpan fireAfter, bool isRescheduleTimer=false):base(false)
        {
            _timerIdentity = timerIdentity;
            _fireAfter = fireAfter;
            _isRescheduleTimer = isRescheduleTimer;
        }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_timerIdentity);
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

        internal override Decision SwfDecision()
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

        public override string ToString()
        {
            return string.Format("{0} for {1} and reschedulable timer is {2} and interval {3}",GetType().Name,_timerIdentity,_isRescheduleTimer, _fireAfter);
        }
    }
}