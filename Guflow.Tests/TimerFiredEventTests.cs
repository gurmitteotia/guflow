using System;
using System.Linq;
using Amazon.SimpleWorkflow;
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
            _timerFiredEvent = CreateTimerFiredEvent(Identity.Timer(_timerName), _fireAfter);
        }

        [Test]
        public void Should_populate_properties_from_timer_event_attributes()
        {
            Assert.That(_timerFiredEvent.Name,Is.EqualTo(_timerName));
            Assert.That(_timerFiredEvent.FiredAfter, Is.EqualTo(_fireAfter));
        }

        [Test]
        public void Throws_exception_when_fired_timer_is_not_found_in_workflow()
        {
            var workflow = new EmptyWorkflow();
            Assert.Throws<IncompatibleWorkflowException>(() => _timerFiredEvent.Interpret(workflow));
        }

        [Test]
        public void Throws_exception_when_timer_start_event_is_missing()
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer(_timerName), _fireAfter);
            Assert.Throws<IncompleteEventGraphException>(()=> new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph.Where(h=>h.EventType!=EventType.TimerStarted)));
        }

        [Test]
        public void By_default_return_continue_workflow_action()
        {
            var workflow = new WorkflowWithTimer();

            var workflowAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(new ContinueWorkflowAction(workflow.CompletedItem,null)));
        }

        [Test]
        public void Workflow_can_return_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var actualAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction.Object));
        }

        [Test]
        public void Can_return_reschedule_workflow_action_if_timer_is_fired_to_reschedule_a_workflow_item()
        {
            var workflow = new SingleActivityWorkflow();
            var rescheduleTimer = CreateRescheduleTimerFiredEvent(Identity.New(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion, SingleActivityWorkflow.PositionalName), _fireAfter);

            var workflowAction = rescheduleTimer.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Reschedule(workflow.RescheduleItem)));
        }

        [Test]
        public void Throws_exception_when_rescheduled_item_is_not_found_in_workflow()
        {
            var workflow = new SingleActivityWorkflow();
            var rescheduleTimer = CreateRescheduleTimerFiredEvent(Identity.New("NotIntWorkflow", string.Empty,string.Empty ), _fireAfter);

            Assert.Throws<IncompatibleWorkflowException>(()=> rescheduleTimer.Interpret(workflow));
        }

        private TimerFiredEvent CreateTimerFiredEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(identity, fireAfter);
            return new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }

        private TimerFiredEvent CreateRescheduleTimerFiredEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(identity, fireAfter, true);
            return new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }

        private class EmptyWorkflow : Workflow
        {
        }

        private class WorkflowWithTimer : Workflow
        {
            public WorkflowWithTimer()
            {
                CompletedItem = AddTimer(_timerName);
            }

            public WorkflowItem CompletedItem { get; private set; }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddTimer(_timerName).WhenFired(e => workflowAction);
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public const string PositionalName = "First";
            public SingleActivityWorkflow()
            {
                RescheduleItem = AddActivity(ActivityName, ActivityVersion, PositionalName);
            }
            public WorkflowItem RescheduleItem { get; private set; }
        }
    }
}