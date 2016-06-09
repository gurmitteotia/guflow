using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivityCancellationFailedEventTests
    {
        private const string _activityName = "download";
        private const string _activityVersion = "1.0";
        private const string _cause = "unknown";
        private ActivityCancellationFailedEvent _activityCancellationFailedEvent;

        [SetUp]
        public void Setup()
        {
            var historyEventGraph = HistoryEventFactory.CreateActivityCancellationFailedEventGraph(Identity.New(_activityName,_activityVersion),_cause);
            _activityCancellationFailedEvent = new ActivityCancellationFailedEvent(historyEventGraph.First());
        }

        [Test]
        public void Throws_exception_when_activity_is_not_found_in_workflow()
        {
           Assert.Throws<IncompatibleWorkflowException>(()=>  _activityCancellationFailedEvent.Interpret(new EmptyWorkflow()));
        }

        [Test]
        public void Should_populate_the_properties_from_event_attributes()
        {
            Assert.That(_activityCancellationFailedEvent.Cause,Is.EqualTo(_cause));
        }

        [Test]
        public void By_default_return_workflow_fail_action()
        {
            var workflowAction = _activityCancellationFailedEvent.Interpret(new TestWorkflow());

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("ACTIVITY_CANCELLATION_FAILED",_cause)));
        }
        [Test]
        public void Can_return_custom_workflow_action()
        {
            var customAction = new Mock<WorkflowAction>();
            var workflowAction = _activityCancellationFailedEvent.Interpret(new WorkflowReturnCustomAction(customAction.Object));

            Assert.That(workflowAction,Is.EqualTo(customAction.Object));
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleActivity(_activityName, _activityVersion);
            }
        }

        private class WorkflowReturnCustomAction : Workflow
        {
            public WorkflowReturnCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion).OnFailedCancellation(c => workflowAction);
            }
        }
    }
}