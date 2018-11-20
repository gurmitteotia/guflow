// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelTimerDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new CancelTimerDecision(Identity.Timer("timer").ScheduleId()).Equals(new CancelTimerDecision(Identity.Timer("timer").ScheduleId())));

            Assert.IsFalse(new CancelTimerDecision(Identity.Timer("timer").ScheduleId()).Equals(new CancelTimerDecision(Identity.Timer("timer1").ScheduleId())));
        }

        [Test]
        public void Should_return_aws_decision_to_cancel_timer()
        {
            var scheduleId = Identity.Timer("timer").ScheduleId();
            var cancelTimerDecision = new CancelTimerDecision(scheduleId);

            Decision swfDecision = cancelTimerDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.CancelTimer));
            Assert.That(swfDecision.CancelTimerDecisionAttributes.TimerId,Is.EqualTo(scheduleId.ToString()));
        }
    }
}