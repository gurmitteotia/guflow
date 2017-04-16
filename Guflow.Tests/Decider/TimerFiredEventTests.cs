using System;
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerFiredEventTests
    {
        private TimerFiredEvent _timerFiredEvent;
        private const string _timerName = "timer1";
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private readonly TimeSpan _fireAfter = TimeSpan.FromSeconds(20);

        [SetUp]
        public void Setup()
        {
            _timerFiredEvent = CreateTimerFiredEvent(Identity.Timer(_timerName), _fireAfter);
        }

        [Test]
        public void Should_not_be_active()
        {
            Assert.That(_timerFiredEvent.IsActive,Is.False);
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

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.ContinueWorkflow(TimerItem.New(Identity.Timer(_timerName),null))));
        }

        [Test]
        public void Workflow_can_return_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomAction(workflowAction);

            var actualAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction));
        }

        [Test]
        public void Should_return_schedule_workflow_action_if_timer_is_fired_to_reschedule_a_activity_item()
        {
            var workflow = new SingleActivityWorkflow();
            var rescheduleTimer = CreateRescheduleTimerFiredEvent(Identity.New(_activityName, _activityVersion, _positionalName), _fireAfter);

            var workflowAction = rescheduleTimer.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Schedule(new ActivityItem(Identity.New(_activityName, _activityVersion, _positionalName), null))));
        }

        [Test]
        public void Should_return_schedule_workflow_action_if_timer_is_fired_to_reschedule_a_timer_item()
        {
            var workflow = new WorkflowWithTimer();
            var rescheduleTimer = CreateRescheduleTimerFiredEvent(Identity.Timer(_timerName), _fireAfter);

            var workflowAction = rescheduleTimer.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Schedule(TimerItem.New(Identity.Timer(_timerName),null))));
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
                ScheduleTimer(_timerName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(_timerName).OnFired(e => workflowAction);
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName);
            }
       }
    }
}