﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;


namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelWorkflowActionTests
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
        public void Should_return_cancel_workflow_decision()
        {
            var action = WorkflowAction.CancelWorkflow("detail");

            var decision = action.Decisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Serialize_complex_detail_object_to_json()
        {
            var action = WorkflowAction.CancelWorkflow(new { Id = 10, Name = "hello" });

            var decision = action.Decisions();

            Assert.That(decision, Is.EquivalentTo(new[] { new CancelWorkflowDecision(@"{""Id"":10,""Name"":""hello""}") }));
        }

        [Test]
        public void Can_be_returned_as_custom_action_in_workflow()
        {
            var workflow = new SingleActivityWorkflow("detail");
            var activityIdentity = Identity.New(ActivityName,ActivityVersion, PositionalName).ScheduleId();
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(activityIdentity, "id", "res");
            var completedActivityEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);

            var decisions = completedActivityEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new[]{new CancelWorkflowDecision("detail")}));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow(string detail)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelWorkflow(detail));
            }
        }
    }
}
