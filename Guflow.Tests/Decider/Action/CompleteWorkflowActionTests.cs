// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CompleteWorkflowActionTests
    {
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Should_return_complete_workflow_decision()
        {
            var workflowAction = WorkflowAction.CompleteWorkflow("result");

            var decision = workflowAction.Decisions();

            Assert.That(decision,Is.EquivalentTo(new []{new CompleteWorkflowDecision("result")}));
        }

        [Test]
        public void Serialize_complex_result_to_json()
        {
            var workflowAction = WorkflowAction.CompleteWorkflow(new {Id =10, Name = "hello"});

            var decision = workflowAction.Decisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new CompleteWorkflowDecision(@"{""Id"":10,""Name"":""hello""}")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new WorkflowReturningCompleteWorkflowAction("result");
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(Identity.New(WorkflowReturningCompleteWorkflowAction.ActivityName, WorkflowReturningCompleteWorkflowAction.ActivityVersion, WorkflowReturningCompleteWorkflowAction.PositionalName), "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var decisions = completedActivityEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new CompleteWorkflowDecision("result")}));
        }

        private class WorkflowReturningCompleteWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public WorkflowReturningCompleteWorkflowAction(string result)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CompleteWorkflow(result));
            }
        }
    }
}
