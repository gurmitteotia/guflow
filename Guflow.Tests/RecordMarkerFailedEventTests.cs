using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class RecordMarkerFailedEventTests
    {
        private RecordMarkerFailedEvent _recordMarkerFailedEvent;
        [SetUp]
        public void Setup()
        {
            _recordMarkerFailedEvent = new RecordMarkerFailedEvent(HistoryEventFactory.CreateRecordMarkerFailedEvent("marker1","cause"));
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
            var workflowAction = _recordMarkerFailedEvent.Interpret(new EmptyWorkflow());
            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("FAILED_TO_RECORD_MARKER","cause")));
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

            [RecordMarkerFailed]
            protected WorkflowAction OnFailToRecordMarker(RecordMarkerFailedEvent recordMarkerFailedEvent)
            {
                return _workflowAction;
            }
        }
    }
}