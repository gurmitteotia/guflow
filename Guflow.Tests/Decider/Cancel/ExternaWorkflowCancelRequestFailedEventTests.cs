﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ExternalWorkflowCancelRequestFailedEventTests
    {
        private ExternalWorkflowCancelRequestFailedEvent _cancelRequestFailedEvent;
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var identity = Identity.New("w", "v").ScheduleId();
            _cancelRequestFailedEvent = new ExternalWorkflowCancelRequestFailedEvent(_builder.ExternalWorkflowCancelRequestFailedEvent(identity,"rid", "cause").First());
        }

        [Test]
        public void Populate_properties_from_swf_event()
        {
            Assert.That(_cancelRequestFailedEvent.Cause,Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_returns_fail_workflow_decision_when_interpreted()
        {
            var decisions = _cancelRequestFailedEvent.Interpret(new EmptyWorkflow()).Decisions(Mock.Of<IWorkflow>());

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
            public WorkflowAction OnCancelRequestFailed(ExternalWorkflowCancelRequestFailedEvent @event)
            {
                return _workflowAction;
            }
        }
    }
}