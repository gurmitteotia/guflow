// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class FailWorkflowActionTests
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
        public void Should_return_fail_workflow_decision()
        {
            var action = WorkflowAction.FailWorkflow("reason", "detail");

            var decision = action.Decisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Serialize_complex_detail_object_to_json()
        {
            var action = WorkflowAction.FailWorkflow("reason", new { Id = 10, Name = "hello" });

            var decision = action.Decisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new FailWorkflowDecision("reason", @"{""Id"":10,""Name"":""hello""}") }));
        }


        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new SingleActivityWorkflow("reason","detail");
            var activityIdentity = Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(activityIdentity, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var decisions = completedActivityEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("reason", "detail")}));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow(string reason, string detail)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => FailWorkflow(reason, detail));
            }
        }
    }
}