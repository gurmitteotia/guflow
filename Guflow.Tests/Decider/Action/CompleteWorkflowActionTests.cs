// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CompleteWorkflowActionTests
    {
        private EventGraphBuilder _builder;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
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
            var activityIdentity = Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(activityIdentity, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var decisions = completedActivityEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new CompleteWorkflowDecision("result")}));
        }

        private class WorkflowReturningCompleteWorkflowAction : Workflow
        {
            public WorkflowReturningCompleteWorkflowAction(string result)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CompleteWorkflow(result));
            }
        }
    }
}
