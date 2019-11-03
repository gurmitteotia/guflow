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
    //TODO: Update all tests to use CancelRequest APIs.
    [TestFixture]
    public class CancelRequestTests
    {
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string TimerName = "timer2";
        private const string WorkflowName = "Workflow";
        private const string WorkflowVersion = "1.0";
        private const string WorkflowPosName = "pos";
        private Mock<IWorkflow> _workflow;
        private EventGraphBuilder _eventGraph;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _eventGraph = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(new[] { new HistoryEvent() }));
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Returns_cancel_timer_decision_for_timer_item_when_it_is_active()
        {
            SetupWorkflowToReturns(_eventGraph.TimerStartedGraph(Identity.Timer("TimerName").ScheduleId(), TimeSpan.FromSeconds(2)));
            var timerItem = TimerItem.New(Identity.Timer("TimerName"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(timerItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.Timer("TimerName").ScheduleId()) }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_timer_item_when_it_is_active_with_reset_schedule_id()
        {
            const string runId = "runid";
            _builder.AddWorkflowRunId(runId);
            var scheduleId = Identity.Timer("TimerName").ScheduleId(runId+"Reset");
            _builder.AddProcessedEvents(_eventGraph.TimerStartedGraph(scheduleId, TimeSpan.FromSeconds(2)).ToArray());
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());
            var timerItem = TimerItem.New(Identity.Timer("TimerName"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(timerItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(scheduleId) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_reschedule_timer_is_not_active()
        {
            SetupWorkflowToReturns(_eventGraph.ActivityStartedGraph(Identity.New("activityName1", "ver").ScheduleId(), "id"));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver").ScheduleId()) }));
        }

        [Test]
        public void Returns_cancel_activity_decision_for_activity_item_when_neither_activity_not_reschedule_timer_are_active()
        {
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelActivityDecision(Identity.New("activityName1", "ver").ScheduleId()) }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_activity_item_when_reschedule_timer_is_active()
        {
            SetupWorkflowToReturns(_eventGraph.TimerStartedGraph(Identity.New("activityName1", "ver").ScheduleId(), TimeSpan.FromSeconds(2), true));
            var activityItem = new ActivityItem(Identity.New("activityName1", "ver"), _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(activityItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.New("activityName1", "ver").ScheduleId()) }));
        }

        [Test]
        public void Returns_cancel_child_workflow_decision_for_child_workflow_item_when_reschedule_timer_is_not_active()
        {
            var identity = Identity.New("workflow", "ver");
            SetupWorkflowToReturns(_eventGraph.ChildWorkflowStartedEventGraph(identity.ScheduleId(), "id", "input"));
            var childWorkflowItem = new ChildWorkflowItem(identity, _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(childWorkflowItem);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelRequestWorkflowDecision(identity.ScheduleId(), "id")}));
        }

        [Test]
        public void Returns_cancel_child_workflow_decision_for_child_workflow_item_when_neither_child_workflow_not_reschedule_timer_are_active()
        {
            var identity = Identity.New("workflow", "ver");
            var item = new ChildWorkflowItem(identity, _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(item);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelRequestWorkflowDecision(identity.ScheduleId(), null) }));
        }

        [Test]
        public void Returns_cancel_timer_decision_for_child_workflow_item_when_reschedule_timer_is_active()
        {
            var identity = Identity.New("workflow", "ver");
            SetupWorkflowToReturns(_eventGraph.TimerStartedGraph(identity.ScheduleId(), TimeSpan.FromSeconds(2), true));
            var item = new ChildWorkflowItem(identity, _workflow.Object);
            var workflowAction = WorkflowAction.Cancel(item);

            var decisions = workflowAction.Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(identity.ScheduleId()) }));
        }

        [Test]
        public void Cancel_request_for_activity_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelActivityAction();
            var events = CreateCompletedActivityEvent(ActivityName, ActivityVersion, PositionalName);

            var decisions = workflow.Decisions(events);

            Assert.That(decisions, Is.EqualTo(new []{new CancelActivityDecision(Identity.New("ActivityToCancel", "1.2").ScheduleId())}));
        }

        [Test]
        public void Cancel_request_for_timer_can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            var events = CreateCompletedActivityEvent(ActivityName, ActivityVersion, PositionalName);

            var decisions = workflow.Decisions(events);

            Assert.That(decisions, Is.EqualTo(new []{new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId())}));
        }

        [Test]
        public void Cancel_request_for_multiple_items_can_be_returned_as_workflow_action()
        {
            var workflow = new WorkflowtoReturnCancelActionForMultipleItems();
            var historyEvents = new WorkflowHistoryEvents(new[]{_eventGraph.WorkflowCancellationRequestedEvent("cause")});

            var decisions = workflow.Decisions(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()), new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId())
            }));
        }

        [Test]
        public void By_default_cancel_request_for_timer_does_not_generate_additional_workflow_action_when_timer_is_active()
        {
            var workflow = new WorkflowToReturnCancelledTimerAction();
            _builder.AddNewEvents(TimerStartedEventGraph(TimerName));
            _builder.AddNewEvents(CompletedActivityEventGraph(ActivityName, ActivityVersion,
                PositionalName));
          
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()) }));
        }

        [Test]
        public void During_timer_cancel_request_user_can_provide_additional_cancel_action_when_timer_is_active()
        {
            var additionalAction = new Mock<WorkflowAction>();
            additionalAction.Setup(a => a.Decisions(It.IsAny<IWorkflow>())).Returns(new[] {new RecordMarkerWorkflowDecision("result" ,"details"), });
            var workflow = new WorkflowToReturnCustomActionDuringCancel(additionalAction.Object);
            _builder.AddNewEvents(TimerStartedEventGraph(TimerName));
            _builder.AddNewEvents(CompletedActivityEventGraph(ActivityName, ActivityVersion,
                PositionalName));

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
                new RecordMarkerWorkflowDecision("result" ,"details"), 
            }));
        }

        [Test]
        public void During_timer_cancel_request_user_provided_additional_cancel_action_is_ignored_when_timer_not_active()
        {
            var additionalAction = new Mock<WorkflowAction>();
            additionalAction.Setup(a => a.Decisions(Mock.Of<IWorkflow>())).Returns(new[] { new RecordMarkerWorkflowDecision("result", "details"), });
            var workflow = new WorkflowToReturnCustomActionDuringCancel(additionalAction.Object);
            _builder.AddNewEvents(CompletedActivityEventGraph(ActivityName, ActivityVersion,
                PositionalName));

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
            }));
        }

        [Test]
        public void Can_invoke_cancel_request_for_timer_in_timer_oncancel_api()
        {
            var workflow = new InvokedCancelRequestForTimerInOnCancelMethod();
            _builder.AddNewEvents(TimerStartedEventGraph(TimerName));
            _builder.AddNewEvents(CompletedActivityEventGraph(ActivityName, ActivityVersion,
                PositionalName));

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
            }));
        }

        [Test]
        public void Returns_cancel_request_for_child_workflow()
        {
            const string parentRunId = "ParentRunId";
            var workflow = new CancelRequestForChildWorkflow();
            _builder.AddProcessedEvents(ChildWorkflowStarted(parentRunId, "rid"));
            _builder.AddWorkflowRunId(parentRunId);
            _builder.AddNewEvents(TimerFiredEventGraph(TimerName));

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]{new CancelRequestWorkflowDecision(Identity.New(WorkflowName, WorkflowVersion, WorkflowPosName).ScheduleId(parentRunId), "rid")}));
        }

        [Test]
        public void Returns_cancel_request_for_child_workflow_using_generic_api()
        {
            const string parentRunId = "ParentRunId";

            var workflow = new CancelRequestForChildWorkflowUsingGenericTypeApi();
            _builder.AddProcessedEvents(ChildWorkflowStarted(parentRunId, "rid"));
            _builder.AddWorkflowRunId(parentRunId);
            _builder.AddNewEvents(TimerFiredEventGraph(TimerName));

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CancelRequestWorkflowDecision(Identity.New(WorkflowName, WorkflowVersion, WorkflowPosName).ScheduleId(parentRunId), "rid") }));
        }

        [Test]
        public void Invalid_arugments_test()
        {
            var cancelRequest = new CancelRequest(null);

            Assert.Throws<ArgumentException>(() => cancelRequest.ForActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => cancelRequest.ForActivity("activity", null));
            Assert.Throws<ArgumentException>(() => cancelRequest.ForTimer(null));
            Assert.Throws<ArgumentException>(() => cancelRequest.ForWorkflow(null));
            Assert.Throws<ArgumentNullException>(() => cancelRequest.For(null));
            Assert.Throws<ArgumentException>(() => cancelRequest.ForChildWorkflow(null, "1.0"));
            Assert.Throws<ArgumentException>(() => cancelRequest.ForChildWorkflow("name", null));
        }

        private IWorkflowHistoryEvents CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = _eventGraph.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName).ScheduleId(), "id", "res");
            return new WorkflowHistoryEvents(allHistoryEvents);
        }

        private HistoryEvent[] CompletedActivityEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return _eventGraph.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName).ScheduleId(), "id", "res").ToArray();
        }

        private HistoryEvent[] TimerStartedEventGraph(string timerName)
        {
            return _eventGraph.TimerStartedGraph(Identity.Timer(timerName).ScheduleId(), TimeSpan.FromSeconds(1)).ToArray();
        }

        private HistoryEvent[] TimerFiredEventGraph(string timerName)
        {
            return _eventGraph.TimerFiredGraph(Identity.Timer(timerName).ScheduleId(), TimeSpan.FromSeconds(1)).ToArray();
        }

        private HistoryEvent[] ChildWorkflowStarted(string parentRunId, string runId)
        {
            return _eventGraph
                .ChildWorkflowStartedEventGraph(Identity.New(WorkflowName, WorkflowVersion, WorkflowPosName).ScheduleId(parentRunId), runId, "input").ToArray();
        }

        private class WorkflowToReturnCancelActivityAction : Workflow
        {
            public WorkflowToReturnCancelActivityAction()
            {
                ScheduleActivity("ActivityToCancel", "1.2");
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelRequest.ForActivity("ActivityToCancel", "1.2"));
            }
        }

        private class WorkflowtoReturnCancelActionForMultipleItems : Workflow
        {
            public WorkflowtoReturnCancelActionForMultipleItems()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
                ScheduleTimer(TimerName);
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
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelRequest.ForTimer(TimerName));
                ScheduleTimer(TimerName);
            }
        }

        private class WorkflowToReturnCustomActionDuringCancel : Workflow
        {
            public WorkflowToReturnCustomActionDuringCancel(WorkflowAction action)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelRequest.For(Timer(TimerName)));
                ScheduleTimer(TimerName).OnCancel(_ => action);
            }
        }

        private class InvokedCancelRequestForTimerInOnCancelMethod : Workflow
        {
            public InvokedCancelRequestForTimerInOnCancelMethod()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => CancelRequest.ForTimer(TimerName));
                ScheduleTimer(TimerName).OnCancel(_ => CancelRequest.ForTimer(TimerName));
            }
        }

        private class CancelRequestForChildWorkflow : Workflow
        {
            public CancelRequestForChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, WorkflowPosName);
                ScheduleTimer(TimerName).OnFired(_ => CancelRequest.ForChildWorkflow(WorkflowName, WorkflowVersion, WorkflowPosName));
            }
        }

        private class CancelRequestForChildWorkflowUsingGenericTypeApi : Workflow
        {
            public CancelRequestForChildWorkflowUsingGenericTypeApi()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, WorkflowPosName);
                ScheduleTimer(TimerName).OnFired(_ => CancelRequest.ForChildWorkflow<ChildWorkflow>(WorkflowPosName));
            }
        }

        [WorkflowDescription(WorkflowVersion, Name = WorkflowName)]
        private class ChildWorkflow : Workflow
        {

        }
    }
}