// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class FailWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new FailWorkflowDecision("reason","detail").Equals(new FailWorkflowDecision("reason","detail")));
            Assert.True(new FailWorkflowDecision("", "").Equals(new FailWorkflowDecision("", "")));
            Assert.True(new FailWorkflowDecision(null, null).Equals(new FailWorkflowDecision(null,null)));
        }

        [Test]
        public void Inequality_tests()
        {
            Assert.False(new FailWorkflowDecision("reason", "detail").Equals(new FailWorkflowDecision("reason", "detail2")));
            Assert.False(new FailWorkflowDecision("reason1", "detail").Equals(new FailWorkflowDecision("reason", "detail")));
            Assert.False(new FailWorkflowDecision("", "").Equals(new FailWorkflowDecision("reason", "detail")));
        }

        [Test]
        public void Return_aws_workflow_fail_decision()
        {
            var failWorkflowDecision = new FailWorkflowDecision("reason","detail");

            var decision = failWorkflowDecision.SwfDecision();

            Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.FailWorkflowExecution));
            Assert.That(decision.FailWorkflowExecutionDecisionAttributes,Is.Not.Null);
            Assert.That(decision.FailWorkflowExecutionDecisionAttributes.Reason,Is.EqualTo("reason"));
            Assert.That(decision.FailWorkflowExecutionDecisionAttributes.Details, Is.EqualTo("detail"));
        }
    }
}