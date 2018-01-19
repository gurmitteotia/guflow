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
            Assert.IsTrue(new CancelTimerDecision(Identity.Timer("timer")).Equals(new CancelTimerDecision(Identity.Timer("timer"))));

            Assert.IsFalse(new CancelTimerDecision(Identity.Timer("timer")).Equals(new CancelTimerDecision(Identity.Timer("timer1"))));
        }

        [Test]
        public void Should_return_aws_decision_to_cancel_timer()
        {
            var timerIdentity = Identity.Timer("timer");
            var cancelTimerDecision = new CancelTimerDecision(timerIdentity);

            Decision swfDecision = cancelTimerDecision.Decision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.CancelTimer));
            Assert.That(swfDecision.CancelTimerDecisionAttributes.TimerId,Is.EqualTo(timerIdentity.Id.ToString()));
        }
    }
}