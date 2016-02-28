using System;
using System.Linq;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerFailedEventTests
    {
        private TimerFailedEvent _timerFailedEvent;
        private const string _timerFailureCause = "something fancy";
        [SetUp]
        public void Setup()
        {
            _timerFailedEvent = CreateTimerFailedEvent(Identity.Timer("newTimer"), _timerFailureCause);
        }

        [Test]
        public void Should_populate_the_properties_from_history_event_attributes()
        {
            Assert.That(_timerFailedEvent.Cause,Is.EqualTo(_timerFailureCause));
        }

        [Test]
        public void Should_fail_the_workflow()
        {
            var workflow = new EmptyWorkflow();

            var workflowAction = _timerFailedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("START_TIMER_FAILED",_timerFailureCause)));
        }

        private TimerFailedEvent CreateTimerFailedEvent(Identity timerIdentity, string cause)
        {
            var timerFailedEventGraph = HistoryEventFactory.CreateTimerFailedEventGraph(timerIdentity, cause);
            return new TimerFailedEvent(timerFailedEventGraph.First(), timerFailedEventGraph);
        }
    }
}