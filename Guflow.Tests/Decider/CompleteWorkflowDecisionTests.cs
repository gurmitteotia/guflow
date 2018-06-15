// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CompleteWorkflowDecisionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.True(new CompleteWorkflowDecision("result").Equals(new CompleteWorkflowDecision("result")));
            Assert.True(new CompleteWorkflowDecision(null).Equals(new CompleteWorkflowDecision(null)));
        }

        [Test]
        public void Inequality_tests()
        {
            Assert.False(new CompleteWorkflowDecision("result").Equals(new CompleteWorkflowDecision("different")));
            Assert.False(new CompleteWorkflowDecision(null).Equals(new CompleteWorkflowDecision("d")));
        }

        [Test]
        public void Return_aws_workflow_complete_decision()
        {
            var completeDecision = new CompleteWorkflowDecision("result");
            
            Decision decision = completeDecision.SwfDecision();

            Assert.That(decision.CompleteWorkflowExecutionDecisionAttributes,Is.Not.Null);
            Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.CompleteWorkflowExecution));
            Assert.That(decision.CompleteWorkflowExecutionDecisionAttributes.Result,Is.EqualTo("result"));
        }
    }
}