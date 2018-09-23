// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        private EventGraphBuilder _eventGraph;
        private HistoryEventsBuilder _historyBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraph = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(new[] { new HistoryEvent() }));
            _historyBuilder = new HistoryEventsBuilder();
        }

        [Test]
        public void Returns_cancel_timer_decision_for_timer_item_when_it_is_active()
        {
            SetupWorkflowToReturns(_eventGraph.TimerStartedGraph(Identity.Timer("TimerName"), TimeSpan.FromSeconds(2)));
            var timerItem = TimerItem.New(Identity.Timer("TimerName"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(timerItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.Timer("TimerName")) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_reschedule_timer_is_not_active()
        {
            SetupWorkflowToReturns(_eventGraph.ActivityStartedGraph(Identity.New("activityName1", "ver"), "id"));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_neither_activity_not_reschedule_timer_are_active()
        {
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_activity_item_when_reschedule_timer_is_active()
        {
            SetupWorkflowToReturns(_eventGraph.TimerStartedGraph(Identity.New("activityName1", "ver"), TimeSpan.FromSeconds(2), true));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.New("activityName1", "ver")) }));
        }

        [Test]
        public void Cancel_request_for_activity_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelActivityAction();
            var events = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.Decisions(events);

            Assert.That(decisions, Is.EqualTo(new []{new CancelActivityDecision(Identity.New("ActivityToCancel", "1.2"))}));
        }

        [Test]
        public void Cancel_request_for_timer_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            var events = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);

            var decisions = workflow.Decisions(events);

            Assert.That(decisions, Is.EqualTo(new []{new CancelTimerDecision(Identity.Timer(_timerName))}));
        }

        [Test]
        public void Cancel_request_for_multiple_items_can_be_returned_as_workflow_action()
        {
            var workflow = new WorkflowtoReturnCancelActionForMultipleItems();
            var historyEvents = new WorkflowHistoryEvents(new[]{_eventGraph.WorkflowCancellationRequestedEvent("cause")});

            var decisions = workflow.Decisions(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[] { new CancelActivityDecision(Identity.New(_activityName, _activityVersion)), new CancelTimerDecision(Identity.Timer(_timerName)) }));
        }

        [Test]
        public void By_default_cancel_request_for_timer_does_not_generate_additional_workflow_action_when_timer_is_active()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            _historyBuilder.AddNewEvents(TimerStartedEventGraph(_timerName));
            _historyBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion,
                _positionalName));
          
            var decisions = workflow.Decisions(_historyBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.Timer(_timerName)) }));
        }

        [Test]
        public void During_timer_cancel_request_user_can_provide_additional_cancel_action_when_timer_is_active()
        {
            var additionalAction = new Mock<WorkflowAction>();
            additionalAction.Setup(a => a.Decisions()).Returns(new[] {new RecordMarkerWorkflowDecision("result" ,"details"), });
            var workflow = new WorkflowToReturnCustomActionDuringCancel(additionalAction.Object);
            _historyBuilder.AddNewEvents(TimerStartedEventGraph(_timerName));
            _historyBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion,
                _positionalName));

            var decisions = workflow.Decisions(_historyBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(_timerName)),
                new RecordMarkerWorkflowDecision("result" ,"details"), 
            }));
        }

        [Test]
        public void During_timer_cancel_request_user_provided_additional_cancel_action_is_ignored_when_timer_not_active()
        {
            var additionalAction = new Mock<WorkflowAction>();
            additionalAction.Setup(a => a.Decisions()).Returns(new[] { new RecordMarkerWorkflowDecision("result", "details"), });
            var workflow = new WorkflowToReturnCustomActionDuringCancel(additionalAction.Object);
            _historyBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion,
                _positionalName));

            var decisions = workflow.Decisions(_historyBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(_timerName)),
            }));
        }

        [Test]
        public void Can_invoke_cancel_request_for_timer_in_timer_oncancel_api()
        {
            var workflow = new InvokedCancelRequestForTimerInOnCancelMethod();
            _historyBuilder.AddNewEvents(TimerStartedEventGraph(_timerName));
            _historyBuilder.AddNewEvents(CompletedActivityEventGraph(_activityName, _activityVersion,
                _positionalName));

            var decisions = workflow.Decisions(_historyBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(_timerName)),
            }));
        }

        private IWorkflowHistoryEvents CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = _eventGraph.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res");
            return new WorkflowHistoryEvents(allHistoryEvents);
        }

        private HistoryEvent[] CompletedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return _eventGraph.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res").ToArray();
        }

        private HistoryEvent[] TimerStartedEventGraph(string timerName)
        {
            return _eventGraph.TimerStartedGraph(Identity.Timer(timerName), TimeSpan.FromSeconds(1)).ToArray();
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
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelRequest.ForTimer(_timerName));
                ScheduleTimer(_timerName);
            }
        }

        private class WorkflowToReturnCustomActionDuringCancel : Workflow
        {
            public WorkflowToReturnCustomActionDuringCancel(WorkflowAction action)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelRequest.ForTimer(_timerName));
                ScheduleTimer(_timerName).OnCancel(_ => action);
            }
        }

        private class InvokedCancelRequestForTimerInOnCancelMethod : Workflow
        {
            public InvokedCancelRequestForTimerInOnCancelMethod()
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => CancelRequest.ForTimer(_timerName));
                ScheduleTimer(_timerName).OnCancel(_ => CancelRequest.ForTimer(_timerName));
            }
        }
    }
}