// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowSignalFailedEventTests
    {
        private WorkflowSignalFailedEvent _workflowSignaledEvent;
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _workflowSignaledEvent = new WorkflowSignalFailedEvent(_builder.WorkflowSignalFailedEvent("cause","wid","rid"));
        }
        [Test]
        public void Populate_properties_from_event()
        {
            Assert.That(_workflowSignaledEvent.Cause,Is.EqualTo("cause"));
            Assert.That(_workflowSignaledEvent.WorkflowId,Is.EqualTo("wid"));
            Assert.That(_workflowSignaledEvent.RunId,Is.EqualTo("rid"));
        }

        [Test]
        public void By_default_returns_fail_workflow_decision_when_interpreted()
        {
            var decisions = _workflowSignaledEvent.Interpret(new EmptyWorkflow()).Decisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("FAILED_TO_SIGNAL_WORKFLOW","cause")}));
        }

        [Test]
        public void Can_return_custom_action_when_interpreted()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;

            var actualAction = _workflowSignaledEvent.Interpret(new WorkflowToReturnCustomAction(expectedAction));

            Assert.That(actualAction,Is.EqualTo(expectedAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.SignalFailed)]
            protected WorkflowAction OnFailToSignalWorkflow(WorkflowSignalFailedEvent workflowSignalFailedEvent)
            {
                return _workflowAction;
            }
        }
    }
}