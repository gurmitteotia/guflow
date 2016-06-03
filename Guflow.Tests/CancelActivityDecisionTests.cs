using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class CancelActivityDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new CancelActivityDecision(Identity.New("activity","1.0")).Equals(new CancelActivityDecision(Identity.New("activity","1.0"))));

            Assert.IsFalse(new CancelActivityDecision(Identity.New("activity", "1.0")).Equals(new CancelActivityDecision(Identity.New("activity", "2.0"))));
        }

        [Test]
        public void Return_aws_decision_to_cancel_activity()
        {
            var activityIdentity = Identity.New("activity", "1.0");
            var cancelActivityDecision = new CancelActivityDecision(activityIdentity);

            Decision swfDecision = cancelActivityDecision.Decision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.RequestCancelActivityTask));
            Assert.That(swfDecision.RequestCancelActivityTaskDecisionAttributes.ActivityId,Is.EqualTo(activityIdentity.Id.ToString()));
        }
    }
}