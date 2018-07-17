// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ScheduleLambdaDecisionTests
    {
        [Test]
        public void Equality()
        {
            Assert.IsTrue(new ScheduleLambdaDecision(Identity.Lambda("lambda"), "input1").Equals(new ScheduleLambdaDecision(Identity.Lambda("lambda"), "input2")));

            Assert.IsFalse(new ScheduleLambdaDecision(Identity.Lambda("lambda1"), "input").Equals(new ScheduleLambdaDecision(Identity.Lambda("lambda"), "input")));
            Assert.IsFalse(new ScheduleLambdaDecision(Identity.Lambda("lambda1"), "input").Equals(null));
        }

        [Test]
        public void Return_swf_decision_to_schedule_lambda()
        {
            var identity = Identity.Lambda("lambda", "pos_name");
            var decision = new ScheduleLambdaDecision(identity, "input", TimeSpan.FromSeconds(2));

            var awsDecision = decision.SwfDecision();
            Assert.That(awsDecision.DecisionType, Is.EqualTo(DecisionType.ScheduleLambdaFunction));
            var attr = awsDecision.ScheduleLambdaFunctionDecisionAttributes;
            Assert.That(attr.Name, Is.EqualTo("lambda"));
            Assert.That(attr.Input, Is.EqualTo("input"));
            Assert.That(attr.StartToCloseTimeout, Is.EqualTo("2"));
            Assert.That(attr.Id, Is.EqualTo(identity.Id.ToString()));
            Assert.That(attr.Control.FromJson<ScheduleData>().PN, Is.EqualTo("pos_name"));
        }

        [Test]
        public void Return_swf_decision_without_input_and_timeout()
        {
            var identity = Identity.Lambda("lambda");
            var decision = new ScheduleLambdaDecision(identity, null, null);

            var awsDecision = decision.SwfDecision();
            Assert.That(awsDecision.DecisionType, Is.EqualTo(DecisionType.ScheduleLambdaFunction));
            var attr = awsDecision.ScheduleLambdaFunctionDecisionAttributes;
            Assert.That(attr.Name, Is.EqualTo("lambda"));
            Assert.That(attr.Input, Is.Null);
            Assert.That(attr.StartToCloseTimeout, Is.Null);
            Assert.That(attr.Id, Is.EqualTo(identity.Id.ToString()));
        }
    }
}