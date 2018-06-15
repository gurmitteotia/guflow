// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
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

            var swfDecision = workflowDecision.SwfDecision();

            Assert.That(swfDecision.DecisionType,Is.EqualTo(DecisionType.RequestCancelExternalWorkflowExecution));
            Assert.That(swfDecision.RequestCancelExternalWorkflowExecutionDecisionAttributes.WorkflowId,Is.EqualTo("wid"));
            Assert.That(swfDecision.RequestCancelExternalWorkflowExecutionDecisionAttributes.RunId, Is.EqualTo("rid"));
        }
    }
}