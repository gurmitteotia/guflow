using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelWorkflowItemActionTests
    {
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _timerName = "timer2";

        private Mock<IWorkflow> _workflow;

        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(new[] { new HistoryEvent() }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_timer_item_when_it_is_active()
        {
            SetupWorkflowToReturns(HistoryEventFactory.CreateTimerStartedEventGraph(Identity.Timer("TimerName"), TimeSpan.FromSeconds(2)));
            var timerItem = TimerItem.New(Identity.Timer("TimerName"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(timerItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.Timer("TimerName")) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_reschedule_timer_is_not_active()
        {
            SetupWorkflowToReturns(HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New("activityName1", "ver"), "id"));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_neither_activity_not_reschedule_timer_are_active()
        {
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_activity_item_when_reschedule_timer_is_active()
        {
            SetupWorkflowToReturns(HistoryEventFactory.CreateTimerStartedEventGraph(Identity.New("activityName1", "ver"), TimeSpan.FromSeconds(2), true));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Cancel_request_for_activity_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelActivityAction();
            workflow.NewExecutionFor(new WorkflowHistoryEvents(new[] { new HistoryEvent() }));
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = completedActivityEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelActivityDecision(Identity.New("ActivityToCancel", "1.2"))}));
        }

        [Test]
        public void Cancel_request_for_timer_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            var completedActivityEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = completedActivityEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelTimerDecision(Identity.Timer("SomeTimer"))}));
        }

        [Test]
        public void Cancel_request_for_multiple_items_can_be_returned_as_workflow_action()
        {
            var workflow = new WorkflowtoReturnCancelActionForMultipleItems();
            workflow.NewExecutionFor(new WorkflowHistoryEvents(new[] {new HistoryEvent()}));
            var cancelRequestEvent = new WorkflowCancellationRequestedEvent(HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause"));

            var workflowAction = cancelRequestEvent.Interpret(workflow).GetDecisions();

            Assert.That(workflowAction, Is.EquivalentTo(new WorkflowDecision[] { new CancelActivityDecision(Identity.New(_activityName, _activityVersion)), new CancelTimerDecision(Identity.Timer(_timerName)) }));
        }

        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private class WorkflowToReturnCancelActivityAction : Workflow
        {
            public WorkflowToReturnCancelActivityAction()
            {
                ScheduleActivity("ActivityToCancel", "1.2");
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelRequest.ForActivity("ActivityToCancel", "1.2"));
            }
        }

        private class WorkflowtoReturnCancelActionForMultipleItems : Workflow
        {
            public WorkflowtoReturnCancelActionForMultipleItems()
            {
                ScheduleActivity(_activityName, _activityVersion);
                ScheduleTimer(_timerName);
            }

            [WorkflowEvent(EventName.CancelRequest)]
            private WorkflowAction OnCancelRequest()
            {
                return CancelRequest.For(WorkflowItems);
            }
        }
        private void SetupWorkflowToReturns(IEnumerable<HistoryEvent> historyEvents)
        {
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(historyEvents));
        }
        private class WorkflowToReturnCancelledTimerAction : Workflow
        {
            public WorkflowToReturnCancelledTimerAction()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelRequest.ForTimer("SomeTimer"));
                ScheduleTimer("SomeTimer");
            }
        }
    }
}