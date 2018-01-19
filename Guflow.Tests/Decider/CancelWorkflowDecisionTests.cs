// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
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
            
            Decision decision = cancelWorkflow.Decision();

            Assert.That(decision.CancelWorkflowExecutionDecisionAttributes,Is.Not.Null);
            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.CancelWorkflowExecution));
            Assert.That(decision.CancelWorkflowExecutionDecisionAttributes.Details,Is.EqualTo("details"));
        }
    }
}