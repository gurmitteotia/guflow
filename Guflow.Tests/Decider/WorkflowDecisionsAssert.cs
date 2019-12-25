using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    internal static class WorkflowDecisionsAssert
    {
        public static void AssertRescheduleTimer(this WorkflowDecision decision, ScheduleId scheduleId, TimeSpan timeout)
        {
            Assert.That(decision, Is.EqualTo(ScheduleTimerDecision.RescheduleTimer(scheduleId, timeout)));
            var swfDecision = decision.SwfDecision();
            var timerData = swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>();
            Assert.That(timerData.TimerType, Is.EqualTo(TimerType.Reschedule));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo(timeout.TotalSeconds.ToString()));
        }

        public static void AssertWorkflowItemTimer(this WorkflowDecision decision, ScheduleId scheduleId, TimeSpan timeout)
        {
            Assert.That(decision, Is.EqualTo(ScheduleTimerDecision.WorkflowItem(scheduleId, timeout)));
            var swfDecision = decision.SwfDecision();
            var timerData = swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>();
            Assert.That(timerData.TimerType, Is.EqualTo(TimerType.WorkflowItem));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo(timeout.TotalSeconds.ToString()));
        }

        public static void AssertSignalTimer(this WorkflowDecision decision, ScheduleId scheduleId, long triggerId, TimeSpan timeout)
        {
            Assert.That(decision, Is.EqualTo(ScheduleTimerDecision.SignalTimer(scheduleId, triggerId, timeout)));
            var swfDecision = decision.SwfDecision();
            var timerData = swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>();
            Assert.That(timerData.TimerType, Is.EqualTo(TimerType.SignalTimer));
            Assert.That(timerData.SignalTriggerEventId, Is.EqualTo(triggerId));
            var startToFireTimeout = TimeSpan.FromSeconds(Double.Parse(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout));
            Assert.That(startToFireTimeout-timeout, Is.LessThan(TimeSpan.FromSeconds(1)));
        }

        public static void AssertWaitForSignal(this WorkflowDecision decision, ScheduleId scheduleId, long triggerEventId,
            SignalWaitType waitType, SignalNextAction nextAction, params string[] signalNames)
        {
            Assert.That(decision, Is.EqualTo(new WaitForSignalsDecision(new WaitForSignalData { ScheduleId = scheduleId, TriggerEventId = triggerEventId })));
            var swfDecision = decision.SwfDecision();
            var data = swfDecision.RecordMarkerDecisionAttributes.Details.AsDynamic();
            Assert.That(data.SignalNames.ToObject<string[]>(), Is.EqualTo(signalNames));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(waitType));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(nextAction));
            Assert.That(((DateTime?)data.TriggerEventCompletionDate).HasValue, Is.False);
            Assert.That(((DateTime?)data.Timeout).HasValue, Is.False);
        }

        public static void AssertWaitForSignal(this WorkflowDecision decision, ScheduleId scheduleId, long triggerEventId,
            SignalWaitType waitType, SignalNextAction nextAction, DateTime triggerDateTime, TimeSpan timeout, params string[] signalNames)
        {
            Assert.That(decision, Is.EqualTo(new WaitForSignalsDecision(new WaitForSignalData { ScheduleId = scheduleId, TriggerEventId = triggerEventId })));
            var swfDecision = decision.SwfDecision();
            var data = swfDecision.RecordMarkerDecisionAttributes.Details.AsDynamic();
            Assert.That(data.SignalNames.ToObject<string[]>(), Is.EqualTo(signalNames));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(waitType));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(nextAction));
            Assert.That(((DateTime?)data.TriggerEventCompletionDate).Value, Is.EqualTo(triggerDateTime));
            Assert.That(((TimeSpan?)data.Timeout).Value, Is.EqualTo(timeout));
        }

        public static void AssertWaitForSignal(this WorkflowDecision decision, ScheduleId scheduleId, long triggerEventId,
                                            DateTime triggerDateTime, TimeSpan timeout, params string[] signalNames)
        {
           decision.AssertWaitForSignal(scheduleId, triggerEventId, SignalWaitType.Any, SignalNextAction.Continue, triggerDateTime, timeout, signalNames);
        }

        public static void AssertSignalTimedout(this WorkflowDecision decision, ScheduleId scheduleId, long triggerEventId, string[] signalNames, long timeoutTriggerId)
        {
            Assert.That(decision, Is.EqualTo(new WorkflowItemSignalsTimedoutDecision(scheduleId, triggerEventId, signalNames, timeoutTriggerId)));
            var swfDecision = decision.SwfDecision();
            var data = swfDecision.RecordMarkerDecisionAttributes.Details.AsDynamic();
            Assert.That(data.TimedoutSignalNames.ToObject<string[]>(), Is.EqualTo(signalNames));
            Assert.That((long)data.TimeoutTriggerEventId, Is.EqualTo(timeoutTriggerId));
        }
    }
}