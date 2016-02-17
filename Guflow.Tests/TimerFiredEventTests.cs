using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerFiredEventTests
    {
        private TimerFiredEvent _timerFiredEvent;
        private const string _timerName = "timer1";
        private readonly TimeSpan _fireAfter = TimeSpan.FromSeconds(20);
        [SetUp]
        public void Setup()
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(_timerName,_fireAfter);
            _timerFiredEvent = new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);
        }

        [Test]
        public void Should_populate_properties_from_timer_event_attributes()
        {
            Assert.That(_timerFiredEvent.Name,Is.EqualTo(_timerName));
            Assert.That(_timerFiredEvent.FireAfter, Is.EqualTo(_fireAfter));
        }

        [Test]
        public void Throws_exception_when_fired_timer_is_not_found_in_workflow()
        {
            var workflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _timerFiredEvent.Interpret(workflow));
        }

        [Test]
        public void Return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new WorkflowWithTimer();

            var decisions = _timerFiredEvent.Interpret(workflow).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Can_return_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var actualAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction.Object));
        }

        [Test]
        public void Return_child_workflow_item_decision()
        {
            
        }


        private class EmptyWorkflow : Workflow
        {
        }

        private class WorkflowWithTimer : Workflow
        {
            public WorkflowWithTimer()
            {
                AddTimer(_timerName).FireAfter(TimeSpan.FromSeconds(20));
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddTimer(_timerName).FireAfter(TimeSpan.FromSeconds(20)).WhenFired(e => workflowAction);
            }
        }
    }
}