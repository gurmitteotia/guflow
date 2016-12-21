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
            Assert.IsTrue(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), true).Equals(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), true)));

            Assert.IsFalse(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), true).Equals(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), false)));
            Assert.IsFalse(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), true).Equals(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(3), true)));
            Assert.IsFalse(new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2), true).Equals(new ScheduleTimerDecision(Identity.Timer("timer1"), TimeSpan.FromSeconds(2), true)));
        }

        [Test]
        public void Should_return_aws_decision_to_schedule_timer()
        {
            var timerIdentity = Identity.Timer("timer");
            var scheduleTimerDecision = new ScheduleTimerDecision(timerIdentity, TimeSpan.FromSeconds(2),true);

            var swfDecision = scheduleTimerDecision.Decision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.TimerId,Is.EqualTo(timerIdentity.Id.ToString()));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout,Is.EqualTo("2"));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.FromJson<TimerScheduleData>().IsARescheduleTimer, Is.EqualTo(true));
            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.FromJson<TimerScheduleData>().TimerName, Is.EqualTo("timer"));
        }

        [Test]
        public void Should_round_up_time_to_fire_duration()
        {
            
            var scheduleTimerDecision = new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2.6), true);

            var swfDecision = scheduleTimerDecision.Decision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartTimer));
            Assert.That(swfDecision.StartTimerDecisionAttributes.StartToFireTimeout, Is.EqualTo("3"));
        }

        [Test]
        public void By_default_it_is_not_reschedulable_timer()
        {

            var scheduleTimerDecision = new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2.6));

            var swfDecision = scheduleTimerDecision.Decision();

            Assert.That(swfDecision.StartTimerDecisionAttributes.Control.FromJson<TimerScheduleData>().IsARescheduleTimer, Is.EqualTo(false));
        }
    }
}