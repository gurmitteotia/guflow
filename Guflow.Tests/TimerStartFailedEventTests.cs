using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerStartFailedEventTests
    {
        private TimerStartFailedEvent _timerStartFailedEvent;
        private const string _timerFailureCause = "something fancy";
        private const string _timerName = "timername";
        [SetUp]
        public void Setup()
        {
            _timerStartFailedEvent = CreateTimerFailedEvent(Identity.Timer("newTimer"), _timerFailureCause);
        }

        [Test]
        public void Should_populate_the_properties_from_history_event_attributes()
        {
            Assert.That(_timerStartFailedEvent.Cause,Is.EqualTo(_timerFailureCause));
            Assert.That(_timerStartFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_should_fail_the_workflow()
        {
            var workflow = new EmptyWorkflow();

            var workflowAction = _timerStartFailedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("START_TIMER_FAILED",_timerFailureCause)));
        }

        [Test]
        public void Can_return_the_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var action = _timerStartFailedEvent.Interpret(workflow);

            Assert.That(action,Is.EqualTo(workflowAction.Object));
        }

        private TimerStartFailedEvent CreateTimerFailedEvent(Identity timerIdentity, string cause)
        {
            var timerFailedEventGraph = HistoryEventFactory.CreateTimerFailedEventGraph(timerIdentity, cause);
            return new TimerStartFailedEvent(timerFailedEventGraph.First());
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(_timerName).OnStartFailure(e => workflowAction);
            }
        }
    }
}