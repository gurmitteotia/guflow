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
        
        [SetUp]
        public void Setup()
        {
            var timerCancellationFailedEventGraph =HistoryEventFactory.CreateTimerCancellationFailedEventGraph(Identity.Timer(_timerName), _cause);
            _timerCancellationFailedEvent = new TimerCancellationFailedEvent(timerCancellationFailedEventGraph.First());
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_timerCancellationFailedEvent.Cause,Is.EqualTo(_cause));
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
        public void Can_return_custom_workflow_action_from_workflow()
        {
            var customAction = new Mock<WorkflowAction>();
            var workflowAction = _timerCancellationFailedEvent.Interpret(new WorkflowWithCustomAction(customAction.Object));

            Assert.That(workflowAction, Is.EqualTo(customAction.Object));
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
    }
}