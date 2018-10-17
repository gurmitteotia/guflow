// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowCancelRequestFailedEventTests
    {
        private ExternalWorkflowCancelRequestFailedEvent _cancelRequestFailedEvent;
        private EventGraphBuilder _builder;
        private const string WorkflowName = "Workflow";
        private const string Version = "1.0";

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var identity = Identity.New(WorkflowName, Version);
            var eventGraph = _builder.ExternalWorkflowCancelRequestFailedEvent(identity, "rid", "cause");
            _cancelRequestFailedEvent = new ExternalWorkflowCancelRequestFailedEvent(eventGraph.First());
        }

        [Test]
        public void Populate_properties_from_swf_event()
        {
            Assert.That(_cancelRequestFailedEvent.Cause, Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_fail_workflow_decision_when_interpreted()
        {
            var decisions = _cancelRequestFailedEvent.Interpret(new TestWorkflow()).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("FAILED_TO_SEND_CANCEL_REQUEST", "cause") }));
        }

        [Test]
        public void Can_return_custom_workflow_action_when_interpreted()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _cancelRequestFailedEvent.Interpret(new WorkflowToReturnCustomAction(expectedWorkflowAction));

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction));
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, Version);
            }
        }

        private class TestChildWorkflow : Workflow
        {
            public TestChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, Version)
                    .OnCancellationFailed();
            }
        }
    }
}