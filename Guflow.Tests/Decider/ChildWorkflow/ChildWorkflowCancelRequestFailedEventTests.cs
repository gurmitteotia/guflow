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
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        private const string WorkflowName = "Workflow";
        private const string Version = "1.0";
        private const string ParentWorkflowRunId = "PId";
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder().AddWorkflowRunId(ParentWorkflowRunId);
            var identity = Identity.New(WorkflowName, Version).ScheduleId(ParentWorkflowRunId);
            var eventGraph = _eventGraphBuilder.ExternalWorkflowCancelRequestFailedEvent(identity, "rid", "cause").ToArray();
            _builder.AddNewEvents(eventGraph);
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
            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("FAILED_TO_SEND_CANCEL_REQUEST", "cause") }));
        }

        [Test]
        public void Can_return_custom_workflow_action_when_interpreted()
        {
            var decisions = new TestChildWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]{new CompleteWorkflowDecision("result")}));
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
                    .OnCancellationFailed(_=>CompleteWorkflow("result"));
            }
        }
    }
}