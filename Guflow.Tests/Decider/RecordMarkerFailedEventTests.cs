// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RecordMarkerFailedEventTests
    {
        private RecordMarkerFailedEvent _recordMarkerFailedEvent;
        private EventGraphBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder(); 
            _recordMarkerFailedEvent = new RecordMarkerFailedEvent(_builder.RecordMarkerFailedEvent("marker1","cause"));
        }
        [Test]
        public void Populate_properties_from_event_attributes()
        {
            Assert.That(_recordMarkerFailedEvent.MarkerName,Is.EqualTo("marker1"));
            Assert.That(_recordMarkerFailedEvent.Cause, Is.EqualTo("cause"));
        }
        [Test]
        public void By_default_return_fail_workflow_action()
        {
            var decisions = _recordMarkerFailedEvent.Interpret(new EmptyWorkflow()).Decisions(Mock.Of<IWorkflow>());
            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("FAILED_TO_RECORD_MARKER","cause")}));
        }
        [Test]
        public void Can_return_custom_workflow_action()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;

            var workflowAction = _recordMarkerFailedEvent.Interpret(new WorkflowWithCustomAction(expectedAction));

            Assert.That(workflowAction,Is.EqualTo(expectedAction));
        }
        private class WorkflowWithCustomAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.RecordMarkerFailed)]
            protected WorkflowAction OnFailToRecordMarker(RecordMarkerFailedEvent recordMarkerFailedEvent)
            {
                return _workflowAction;
            }
        }
    }
}