// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerResetTests
    {
        private const string TimerName = "Timer1";
        private const string LambdaName = "LambdaName";
        private const string ParentWorkflowRunId = "runid";
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }

        [Test]
        public void Current_timer_is_cancelled_and_is_scheduled_with_new_scheduled_id_on_reset()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder
                .TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromMinutes(4)).ToArray());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            var decisions = new TimerResetWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
                new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId+"Reset"), TimeSpan.FromMinutes(4))
            }));
        }

        [Test]
        public void Current_timer_is_cancelled_and_is_scheduled_with_new_scheduled_id_and_timeout_on_reschedule()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder
                .TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromMinutes(4)).ToArray());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            var decisions = new TimerResetWithTimeoutWithOldApiWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
                new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId+"Reset"), TimeSpan.FromMinutes(10))
            }));
        }

        [Test]
        public void Current_timer_is_cancelled_and_is_scheduled_with_new_scheduled_id_and_timeout_on_reset()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder
                .TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromMinutes(4)).ToArray());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            var decisions = new TimerResetWithTimeoutWithNewApiWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId()),
                new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId+"Reset"), TimeSpan.FromMinutes(10))
            }));
        }

        [Test]
        public void Current_timer_is_cancelled_and_is_scheduled_with_flipped_scheduled_id_on_reset()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder
                .TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId + "Reset"), TimeSpan.FromMinutes(4)).ToArray());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            var decisions = new TimerResetWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId + "Reset")),
                new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromMinutes(4))
            }));
        }

        [Test]
        public void Current_timer_is_cancelled_and_is_scheduled_with_flipped_scheduled_id_and_timeout_on_reschedule()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder
                .TimerStartedGraph(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId + "Reset"), TimeSpan.FromMinutes(4)).ToArray());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            var decisions = new TimerResetWithTimeoutWithNewApiWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new CancelTimerDecision(Identity.Timer(TimerName).ScheduleId(ParentWorkflowRunId+"Reset")),
                new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromMinutes(10))
            }));
        }

        [Test]
        public void Reset_throws_exception_when_timer_is_not_active()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            Assert.Throws<InvalidOperationException>(()=> new TimerResetWorkflow().Decisions(_eventsBuilder.Result()));
        }

        [Test]
        public void Resechedule_throws_exception_when_timer_is_not_active()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
               _eventsBuilder.AddWorkflowRunId(ParentWorkflowRunId);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowSignaledEvent("ChangeTimer", ""));

            Assert.Throws<InvalidOperationException>(() => new TimerResetWithTimeoutWithNewApiWorkflow().Decisions(_eventsBuilder.Result()));
        }

        private class TimerResetWorkflow : Workflow
        {
            public TimerResetWorkflow()
            {
                ScheduleTimer(TimerName).FireAfter(TimeSpan.FromMinutes(2));

                ScheduleLambda(LambdaName).AfterTimer(TimerName);
            }

           [SignalEvent]
           public WorkflowAction ChangeTimer() => Timer(TimerName).Reset();
        }

        private class TimerResetWithTimeoutWithOldApiWorkflow : Workflow
        {
            public TimerResetWithTimeoutWithOldApiWorkflow()
            {
                ScheduleTimer(TimerName).FireAfter(TimeSpan.FromMinutes(2));

                ScheduleLambda(LambdaName).AfterTimer(TimerName);
            }

            [SignalEvent]
            public WorkflowAction ChangeTimer() => Timer(TimerName).Reschedule(TimeSpan.FromMinutes(10));
        }

        private class TimerResetWithTimeoutWithNewApiWorkflow : Workflow
        {
            public TimerResetWithTimeoutWithNewApiWorkflow()
            {
                ScheduleTimer(TimerName).FireAfter(TimeSpan.FromMinutes(2));

                ScheduleLambda(LambdaName).AfterTimer(TimerName);
            }

            [SignalEvent]
            public WorkflowAction ChangeTimer() => Timer(TimerName).Reset(TimeSpan.FromMinutes(10));
        }
    }
}