// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowCancelledEventTests
    {
        private ChildWorkflowCancelledEvent _event;
        private EventGraphBuilder _eventGraphBuilder;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private Identity _workflowIdentity;

        [SetUp]
        public void Setup()
        {
            _workflowIdentity = Identity.New(WorkflowName, WorkflowVersion, PositionalName);
            _eventGraphBuilder = new EventGraphBuilder();
            var eventGraph =
                _eventGraphBuilder.ChildWorkflowCancelledEventGraph(_workflowIdentity, "rid", "input", "details").ToArray();
            _event = new ChildWorkflowCancelledEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_properties_from_event_graph()
        {
            Assert.That(_event.Details, Is.EqualTo("details"));
            Assert.That(_event.RunId, Is.EqualTo("rid"));
            Assert.That(_event.Input, Is.EqualTo("input"));
        }

        [Test]
        public void By_default_cancel_parent_workflow()
        {
            var decisions = _event.Interpret(new ChildWorkflow()).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelWorkflowDecision("details") }));
        }

        [Test]
        public void Can_return_custom_action()
        {
            var decisions = _event.Interpret(new ChildWorkflowWithCustomAction("result")).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);
            }
        }

        private class ChildWorkflowWithCustomAction : Workflow
        {
            public ChildWorkflowWithCustomAction(string completeResult)
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName)
                    .OnCancelled(_ => CompleteWorkflow(completeResult));
            }
        }
    }
}