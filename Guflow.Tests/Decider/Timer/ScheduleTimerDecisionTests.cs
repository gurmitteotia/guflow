// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ScheduleTimerDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            var scheduleId1 = Identity.Timer("timer1").ScheduleId();
            var scheduleId2 = Identity.Timer("timer2").ScheduleId();

            Assert.IsTrue(ScheduleTimerDecision.RescheduleTimer(scheduleId1, TimeSpan.FromSeconds(2))
                .Equals(ScheduleTimerDecision.RescheduleTimer(scheduleId1, TimeSpan.FromSeconds(3))));

            Assert.IsTrue(ScheduleTimerDecision.WorkflowItem(scheduleId1, TimeSpan.FromSeconds(2))
                .Equals(ScheduleTimerDecision.RescheduleTimer(scheduleId1, TimeSpan.FromSeconds(2))));

            Assert.IsTrue(ScheduleTimerDecision.SignalTimer(scheduleId1, 0,TimeSpan.FromSeconds(2))
                .Equals(ScheduleTimerDecision.RescheduleTimer(scheduleId1,TimeSpan.FromSeconds(2))));

            Assert.IsFalse(ScheduleTimerDecision.RescheduleTimer(scheduleId1, TimeSpan.FromSeconds(2))
                .Equals(ScheduleTimerDecision.RescheduleTimer(scheduleId2, TimeSpan.FromSeconds(2))));

            Assert.IsFalse(ScheduleTimerDecision.RescheduleTimer(scheduleId1, TimeSpan.FromSeconds(2))
                .Equals(ScheduleTimerDecision.WorkflowItem(scheduleId2, TimeSpan.FromSeconds(2))));
            
        }

        [Test]
        public void Hashcode_test()
        {
            var retimer = ScheduleTimerDecision.RescheduleTimer(Identity.Timer("timer1").ScheduleId(), TimeSpan.FromSeconds(2));
            var wtimer = ScheduleTimerDecision.WorkflowItem(Identity.Timer("timer1").ScheduleId(), TimeSpan.FromSeconds(2));
            var sitimer = ScheduleTimerDecision.SignalTimer(Identity.Timer("timer1").ScheduleId(), 1, TimeSpan.FromSeconds(2));
            var retimer2 = ScheduleTimerDecision.RescheduleTimer(Identity.Timer("timer2").ScheduleId(), TimeSpan.FromSeconds(2));
           
            

            Assert.That(retimer.GetHashCode(), Is.EqualTo(wtimer.GetHashCode()));
            Assert.That(retimer.GetHashCode(), Is.EqualTo(sitimer.GetHashCode()));
            Assert.That(retimer.GetHashCode(), Is.Not.EqualTo(retimer2.GetHashCode()));
        }

        [Test]
        public void AWS_decision_for_reschedule_timer()
        {
            var scheduleId = Identity.Timer("timer").ScheduleId();
            var scheduleTimerDecision = ScheduleTimerDecision.RescheduleTimer(scheduleId, TimeSpan.FromSeconds(2));

            var swfDecision = scheduleTimerDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.TimerId,Is.EqualTo(scheduleId.ToString()));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout,Is.EqualTo("2"));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>().TimerType, Is.EqualTo(TimerType.Reschedule));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>().TimerName, Is.EqualTo("timer"));
        }

        [Test]
        public void AWS_decision_for_workflow_item_timer()
        {
            var scheduleId = Identity.Timer("timer").ScheduleId();
            var scheduleTimerDecision = ScheduleTimerDecision.WorkflowItem(scheduleId, TimeSpan.FromSeconds(2));

            var swfDecision = scheduleTimerDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.TimerId, Is.EqualTo(scheduleId.ToString()));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo("2"));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>().TimerType, Is.EqualTo(TimerType.WorkflowItem));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>().TimerName, Is.EqualTo("timer"));
        }

        [Test]
        public void AWS_decision_for_signal_timer()
        {
            var scheduleId = Identity.Timer("timer").ScheduleId();
            long triggerEventId = 10;
            var scheduleTimerDecision = ScheduleTimerDecision.SignalTimer(scheduleId, triggerEventId ,TimeSpan.FromSeconds(2));

            var swfDecision = scheduleTimerDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.TimerId, Is.EqualTo(scheduleId.ToString()));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo("2"));
            var timerScheduleData = swfDecision.StartTimerDecisionAttributes.Control.As<TimerScheduleData>();
            Assert.That(timerScheduleData.TimerType, Is.EqualTo(TimerType.SignalTimer));
            Assert.That(timerScheduleData.TimerName, Is.EqualTo("timer"));
            Assert.That(timerScheduleData.SignalTriggerEventId, Is.EqualTo(triggerEventId));
        }

        [Test]
        public void Should_round_up_time_to_fire_duration()
        {
            
            var scheduleTimerDecision = ScheduleTimerDecision.RescheduleTimer(Identity.Timer("timer").ScheduleId(), TimeSpan.FromSeconds(2.6));

            var swfDecision = scheduleTimerDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo("3"));
        }
    }
}