// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerStartFailedEventTests
    {
        private TimerStartFailedEvent _timerStartFailedEvent;
        private const string _timerFailureCause = "something fancy";
        private const string _timerName = "timername";
        private const string _activityName = "activity";
        private const string _activityVersion = "1.0";
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();

            _timerStartFailedEvent = CreateTimerStartFailedEvent(Identity.Timer(_timerName), _timerFailureCause);
        }

        [Test]
        public void Should_populate_the_properties_from_history_event_attributes()
        {
            Assert.That(_timerStartFailedEvent.Cause,Is.EqualTo(_timerFailureCause));
            Assert.That(_timerStartFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_returns_fail_workflow_decision()
        {
            var workflow = new WorkflowWithTimer();

            var decisions = _timerStartFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("TIMER_START_FAILED", _timerFailureCause)}));
        }

        [Test]
        public void Can_return_the_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var action = _timerStartFailedEvent.Interpret(workflow);

            Assert.That(action,Is.EqualTo(workflowAction.Object));
        }

        [Test]
        public void By_default_fail_workflow_for_reshedule_timer()
        {
            var workflow = new WorkflowWithActivity();
            var rescheduleTimerStartFailed = CreateTimerStartFailedEvent(Identity.New(_activityName,_activityVersion),_timerFailureCause);

            var decisions = rescheduleTimerStartFailed.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("RESCHEDULE_TIMER_START_FAILED", _timerFailureCause)}));
        }

        [Test]
        public void Can_return_custom_action_for_reshedule_timer()
        {
            var expectedWorkflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomRescheduleAction(expectedWorkflowAction.Object);
            var rescheduleTimerStartFailed = CreateTimerStartFailedEvent(Identity.New(_activityName, _activityVersion), _timerFailureCause);

            var workflowAction = rescheduleTimerStartFailed.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(expectedWorkflowAction.Object));
        }

        private TimerStartFailedEvent CreateTimerStartFailedEvent(Identity timerIdentity, string cause)
        {
            var timerFailedEventGraph = _builder.TimerStartFailedGraph(timerIdentity, cause);
            return new TimerStartFailedEvent(timerFailedEventGraph.First());
        }

        private class WorkflowWithActivity : Workflow
        {
            public WorkflowWithActivity()
            {
                ScheduleActivity(_activityName, _activityVersion);
            }
        }
        private class WorkflowWithCustomRescheduleAction : Workflow
        {
            public WorkflowWithCustomRescheduleAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion).RescheduleTimer.OnStartFailure(e => workflowAction);
            }
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
                ScheduleTimer(_timerName).OnStartFailure(e => workflowAction);
            }
        }
    }
}