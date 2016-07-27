using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerCancellationFailedEventTests
    {
        private TimerCancellationFailedEvent _timerCancellationFailedEvent;
        private const string _timerName = "timer";
        private const string _cause = "cause";
        private const string _activityName = "activity";
        private const string _activityVersion = "1.0";
        [SetUp]
        public void Setup()
        {
            _timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.Timer(_timerName), _cause);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_timerCancellationFailedEvent.Cause,Is.EqualTo(_cause));
            Assert.IsFalse(_timerCancellationFailedEvent.IsActive);
        }

        [Test]
        public void Throws_exception_when_timer_is_not_found()
        {
            Assert.Throws<IncompatibleWorkflowException>(() => _timerCancellationFailedEvent.Interpret(new EmptyWorkflow()));
        }

        [Test]
        public void By_default_return_workflow_failed_action()
        {
            var workflowAction = _timerCancellationFailedEvent.Interpret(new TestWorkflow());
            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.FailWorkflow("TIMER_CANCELLATION_FAILED",_cause)));
        }

        [Test]
        public void By_default_return_workflow_failed_action_for_reschedule_timer()
        {
            var workflow = new WorkflowWithActivity();
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.New(_activityName, _activityVersion), _cause);
            var workflowAction = timerCancellationFailedEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.FailWorkflow("RESCHEDULE_TIMER_CANCELLATION_FAILED", _cause)));
        }

        [Test]
        public void Can_return_custom_workflow_action_from_workflow()
        {
            var customAction = new Mock<WorkflowAction>().Object;
            var workflowAction = _timerCancellationFailedEvent.Interpret(new WorkflowWithCustomAction(customAction));

            Assert.That(workflowAction, Is.EqualTo(customAction));
        }
        
        [Test]
        public void Can_return_custom_workflow_action_from_workflow_for_reschedule_timer()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomActionForActivity(expectedWorkflowAction);
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.New(_activityName, _activityVersion), _cause);
            var workflowAction = timerCancellationFailedEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction));
        }

        private TimerCancellationFailedEvent CreateTimerCancellationFailedEvent(Identity identity, string cause)
        {
            var timerCancellationFailedEventGraph = HistoryEventFactory.CreateTimerCancellationFailedEventGraph(identity, _cause);
            return new TimerCancellationFailedEvent(timerCancellationFailedEventGraph.First());
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleTimer(_timerName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(_timerName).OnFailedCancellation(c => workflowAction);
            }
        }

        private class WorkflowWithActivity : Workflow
        {
            public WorkflowWithActivity()
            {
                ScheduleActivity(_activityName, _activityVersion);
            }
        }
        private class WorkflowWithCustomActionForActivity : Workflow
        {
            public WorkflowWithCustomActionForActivity(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion).RescheduleTimer.OnFailedCancellation(e=>workflowAction);
            }
        }
    }
}