// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelActivityDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new CancelActivityDecision(Identity.New("activity","1.0").ScheduleId()).Equals(new CancelActivityDecision(Identity.New("activity","1.0").ScheduleId())));

            Assert.IsFalse(new CancelActivityDecision(Identity.New("activity", "1.0").ScheduleId()).Equals(new CancelActivityDecision(Identity.New("activity", "2.0").ScheduleId())));
        }

        [Test]
        public void Return_aws_decision_to_cancel_activity()
        {
            var scheduleId = Identity.New("activity", "1.0").ScheduleId();
            var cancelActivityDecision = new CancelActivityDecision(scheduleId);

            Decision swfDecision = cancelActivityDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.RequestCancelActivityTask));
            Assert.That(swfDecision.RequestCancelActivityTaskDecisionAttributes.ActivityId,Is.EqualTo(scheduleId.ToString()));
        }
    }
}