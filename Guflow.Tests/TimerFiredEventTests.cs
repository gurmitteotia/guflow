using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerFiredEventTests
    {
        private TimerFiredEvent _timerFiredEvent;
        private const string _timerName = "timer1";
        private const string _childTimerName = "childTimer";
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
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
            Assert.That(_timerFiredEvent.FiredAfter, Is.EqualTo(_fireAfter));
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
        public void By_default_return_continue_workflow_action()
        {
            var workflow = new WorkflowWithTimer();

            var workflowAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(new ContinueWorkflowAction(workflow.CompletedItem,null)));
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
        public void Return_child_workflow_timer_item_decision()
        {
            var workflow = new WorkflowWithMultipleChilds();

            var decisions = _timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new[]{new ScheduleTimerDecision(_childTimerName)}));
        }

        [Test]
        public void Can_return_child_activity_decision()
        {
            var workflow = new WorkflowWithChildActivity();

            var decisions = _timerFiredEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new ScheduleActivityDecision(_activityName,_activityVersion)}));
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

        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                AddTimer(_timerName);
                AddTimer(_childTimerName).DependsOn(_timerName);
            }
        }

        private class WorkflowWithChildActivity : Workflow
        {
            public WorkflowWithChildActivity()
            {
                AddTimer(_timerName);
                AddActivity(_activityName, _activityVersion).DependsOn(_timerName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddTimer(_timerName).WhenFired(e => workflowAction);
            }
        }
    }
}