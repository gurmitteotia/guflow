using Amazon.SimpleWorkflow;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class CancelRequestWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.That(new CancelRequestWorkflowDecision("wid","rid").Equals(new CancelRequestWorkflowDecision("wid","rid")));

            Assert.False(new CancelRequestWorkflowDecision("wid", "rid").Equals(new CancelRequestWorkflowDecision("wid", "rid1")));
            Assert.False(new CancelRequestWorkflowDecision("wid", "rid").Equals(new CancelRequestWorkflowDecision("wid1", "rid")));
        }

        [Test]
        public void Returns_swf_decision_to_request_cancel_workflow()
        {
            var workflowDecision = new CancelRequestWorkflowDecision("wid","rid");

            var swfDecision = workflowDecision.Decision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.RequestCancelExternalWorkflowExecution));
            Assert.That(swfDecision.RequestCancelExternalWorkflowExecutionDecisionAttributes.WorkflowId,Is.EqualTo("wid"));
            Assert.That(swfDecision.RequestCancelExternalWorkflowExecutionDecisionAttributes.RunId, Is.EqualTo("rid"));
        }
    }
}