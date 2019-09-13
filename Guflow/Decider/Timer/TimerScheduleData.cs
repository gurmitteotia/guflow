// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;

namespace Guflow.Decider
{
    internal class TimerScheduleData
    {
        private TimerType _timerType;
        public string TimerName;
        [Obsolete("Only kept for backward compatibility")]
        public bool IsARescheduleTimer;
        public TimerType TimerType
        {
            get => IsARescheduleTimer ? TimerType.Reschedule : _timerType;
            set => _timerType = value;
        }
    }

    internal enum TimerType
    {
        WorkflowItem,
        Reschedule,
        SignalTimer
    }
}