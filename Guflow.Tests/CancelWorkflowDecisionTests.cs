using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class CancelWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new CancelWorkflowDecision("details").Equals(new CancelWorkflowDecision("details")));
            Assert.True(new CancelWorkflowDecision(null).Equals(new CancelWorkflowDecision(null)));
        }

        [Test]
        public void Inequality_tests()
        {
            Assert.False(new CancelWorkflowDecision("details").Equals(new CancelWorkflowDecision("different")));
            Assert.False(new CancelWorkflowDecision(null).Equals(new CancelWorkflowDecision("something")));
        }

        [Test]
        public void Should_return_aws_decision_to_cancel_workflow()
        {
            var cancelWorkflow = new CancelWorkflowDecision("details");
            var decision = cancelWorkflow.Decision();

            Assert.That(decision.CancelWorkflowExecutionDecisionAttributes,Is.Not.Null);
            Assert.That(decision.CancelWorkflowExecutionDecisionAttributes.Details,Is.EqualTo("details"));
        }
    }
}