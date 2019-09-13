// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal sealed class ScheduleTimerDecision : WorkflowDecision
    {
        private readonly ScheduleId _id;
        private readonly TimeSpan _timeout;
        private readonly TimerType _timerType;

        private ScheduleTimerDecision(ScheduleId id, TimeSpan timeout, TimerType timerType) : base(false)
        {
            _id = id;
            _timeout = timeout;
            _timerType = timerType;
        }

        public static ScheduleTimerDecision RescheduleTimer(ScheduleId scheduleId, TimeSpan timeout)
        => new ScheduleTimerDecision(scheduleId, timeout, TimerType.Reschedule);

        public static ScheduleTimerDecision WorkflowItem(ScheduleId scheduleId, TimeSpan timeout)
            => new ScheduleTimerDecision(scheduleId, timeout, TimerType.WorkflowItem);

        public static ScheduleTimerDecision SignalTimer(ScheduleId scheduleId, TimeSpan timeout)
            => new ScheduleTimerDecision(scheduleId, timeout, TimerType.SignalTimer);
        

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_id);
        }

        public override bool Equals(object other)
        {
            var otherTimer = other as ScheduleTimerDecision;
            if (otherTimer == null)
                return false;
            return string.Equals(_id, otherTimer._id) && _timeout.Equals(otherTimer._timeout)
                   && _timerType == otherTimer._timerType;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        internal override Decision SwfDecision()
        {
            return new Decision()
            {
                DecisionType = DecisionType.StartTimer,
                StartTimerDecisionAttributes = new StartTimerDecisionAttributes()
                {
                    TimerId = _id.ToString(),
                    StartToFireTimeout = Math.Round(_timeout.TotalSeconds).ToString(),
                    Control = (new TimerScheduleData() { TimerType = _timerType, TimerName = _id.Name }).ToJson()
                }
            };
        }

        public override string ToString()
        {
            return string.Format("{0} for {1} and timeout type is {2}, interval {3} and id {4}", GetType().Name, _id.Name, _timerType, _timeout, _id);
        }
    }
}