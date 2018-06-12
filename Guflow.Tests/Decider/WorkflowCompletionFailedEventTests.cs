// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowCompletionFailedEventTests
    {
        private WorkflowCompletionFailedEvent _failedEvent;

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();

            _failedEvent = new WorkflowCompletionFailedEvent(_builder.WorkflowCompletionFailureEvent("cause"));
        }

        [Test]
        public void Populates_properties_from_event_graph()
        {
            Assert.That(_failedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_return_fail_workflow_decisions_when_interpreted()
        {
            var decisions = _failedEvent.Interpret(new EmptyWorkflow()).Decisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("FAILED_TO_COMPLETE_WORKFLOW","cause")}));
        }

        [Test]
        public void Can_return_custom_workflow_action_when_interpreted()
        {
            var workflowAction = new Mock<WorkflowAction>().Object;

            var actualWorkflowAction = _failedEvent.Interpret(new WorkflowToReturnCustomAction(workflowAction));

            Assert.That(actualWorkflowAction,Is.EqualTo(workflowAction));
        }

        private class WorkflowToReturnCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowToReturnCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.CompletionFailed)]
            public WorkflowAction OnCompletionFailure(WorkflowCompletionFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}