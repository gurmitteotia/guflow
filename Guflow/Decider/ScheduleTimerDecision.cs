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
        private readonly long _triggerEventId;

        private ScheduleTimerDecision(ScheduleId id, TimeSpan timeout, TimerType timerType, long triggerEventId=0) : base(false)
        {
            _id = id;
            _timeout = timeout;
            _timerType = timerType;
            _triggerEventId = triggerEventId;
        }

        public static ScheduleTimerDecision RescheduleTimer(ScheduleId scheduleId, TimeSpan timeout)
        => new ScheduleTimerDecision(scheduleId, timeout, TimerType.Reschedule);

        public static ScheduleTimerDecision WorkflowItem(ScheduleId scheduleId, TimeSpan timeout)
            => new ScheduleTimerDecision(scheduleId, timeout, TimerType.WorkflowItem);

        public static ScheduleTimerDecision SignalTimer(ScheduleId scheduleId, long triggerEventId ,TimeSpan timeout)
            => new ScheduleTimerDecision(scheduleId, timeout, TimerType.SignalTimer, triggerEventId);
        

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(_id);
        }

        private bool Equals(ScheduleTimerDecision other)
        {
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ScheduleTimerDecision other && Equals(other);
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
                    Control = (new TimerScheduleData() { TimerType = _timerType, TimerName = _id.Name, SignalTriggerEventId = _triggerEventId}).ToJson()
                }
            };
        }

        public override string ToString()
        {
            return string.Format("{0} for {1} and timeout type is {2}, interval {3} and id {4}", GetType().Name, _id.Name, _timerType, _timeout, _id);
        }
    }
}