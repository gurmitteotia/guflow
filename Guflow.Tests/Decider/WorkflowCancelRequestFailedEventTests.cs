// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WorkflowCancelRequestFailedEventTests
    {
        private WorkflowCancelRequestFailedEvent _cancelRequestFailedEvent;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();

            _cancelRequestFailedEvent = new WorkflowCancelRequestFailedEvent(_builder.WorkflowCancelRequestFailedEvent("cause"));
        }

        [Test]
        public void Populate_properties_from_swf_event()
        {
            Assert.That(_cancelRequestFailedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_fail_workflow_decision_when_interpreted()
        {
            var decisions = _cancelRequestFailedEvent.Interpret(new EmptyWorkflow()).GetDecisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("FAILED_TO_SEND_CANCEL_REQUEST","cause")}));
        }

        [Test]
        public void Can_return_custom_workflow_action_when_interpreted()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _cancelRequestFailedEvent.Interpret(new WorkflowToReturnCustomAction(expectedWorkflowAction));

            Assert.That(workflowAction,Is.EqualTo(expectedWorkflowAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.CancelRequestFailed)]
            public WorkflowAction OnCancelRequestFailed(WorkflowCancelRequestFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}