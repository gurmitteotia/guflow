// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowActionTests
    {
        [Test]
        public void Can_combine_two_workflow_actions_using_plus_operator()
        {
            var workflowAction = WorkflowAction.FailWorkflow("reason", "detail") + WorkflowAction.CompleteWorkflow("result");

            var workflowDecisions = workflowAction.Decisions();

            Assert.That(workflowDecisions,Is.EquivalentTo(new WorkflowDecision []{new FailWorkflowDecision("reason","detail"),new CompleteWorkflowDecision("result")}));
        }

        [Test]
        public void Can_combine_two_workflow_actions_using_and_function()
        {
            var workflowAction = WorkflowAction.FailWorkflow("reason", "detail").And(WorkflowAction.CompleteWorkflow("result"));

            var workflowDecisions = workflowAction.Decisions();

            Assert.That(workflowDecisions, Is.EquivalentTo(new WorkflowDecision[] { new FailWorkflowDecision("reason", "detail"), new CompleteWorkflowDecision("result") }));
        }

        [Test]
        public void Throws_exception_when_null_are_passed_combination()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var a = WorkflowAction.FailWorkflow("reason", "detail") + null;
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var a =   null + WorkflowAction.FailWorkflow("reason", "detail");
            });
        }
    }
}