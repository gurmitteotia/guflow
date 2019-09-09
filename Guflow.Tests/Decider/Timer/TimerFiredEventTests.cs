// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        private const string TimerName = "timer1";
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string LambdaName = "Lambda";
        private const string WorkflowName = "Workflow";
        private const string WorkflowVersion = "1.0";
        private readonly TimeSpan _fireAfter = TimeSpan.FromSeconds(20);
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Identity _identity;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent("input"));
            _identity = Identity.Timer(TimerName);

            _timerFiredEvent = CreateTimerFiredEvent(_identity, _fireAfter);
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
            var timerFiredEventGraph = _graphBuilder.TimerFiredGraph(_identity.ScheduleId(), _fireAfter);
            Assert.Throws<IncompleteEventGraphException>(()=> new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph.Where(h=>h.EventType!=EventType.TimerStarted)));
        }

        [Test]
        public void By_default_schedule_children()
        {
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(_identity.ScheduleId(), TimeSpan.Zero).ToArray());
            var workflow = new WorkflowWithTimer();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName).ScheduleId(), "input")}));
        }

        [Test]
        public void Schedule_children_when_reset_timer_is_fired()
        {
            const string runId = "runid";
            _builder.AddWorkflowRunId(runId);
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(_identity.ScheduleId(runId+"Reset"), TimeSpan.Zero).ToArray());
            var workflow = new WorkflowWithTimer();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName).ScheduleId(), "input") }));
        }

        [Test]
        public void Workflow_can_return_custom_action()
        {
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(_identity.ScheduleId(), TimeSpan.Zero).ToArray());

            var workflow = new WorkflowWithCompleteAction("result");

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new[]{new CompleteWorkflowDecision("result")}));
        }


        [Test]
        public void Returns_schedule_activity_decision_if_timer_is_fired_to_reschedule_an_activity_item_using_old_data_object()
        {
            var workflow = new SingleActivityWorkflow();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId(), _fireAfter, true).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId()) }));
        }



        [Test]
        public void Returns_schedule_activity_decision_if_timer_is_fired_to_reschedule_an_activity_item()
        {
            var workflow = new SingleActivityWorkflow();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId(), _fireAfter, TimerType.Reschedule).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new []{new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId()) }));
        }

        [Test]
        public void Returns_schedule_timer_decision_if_timer_is_fired_to_reschedule_a_timer_item()
        {
            var workflow = new WorkflowWithTimer();
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(_identity.ScheduleId(), _fireAfter, TimerType.Reschedule).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new []{new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(), TimeSpan.Zero) }));
        }

        [Test]
        public void Returns_schedule_timer_decision_if_timer_is_fired_to_reschedule_a_timer_item_using_old_data_object()
        {
            var workflow = new WorkflowWithTimer();
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(_identity.ScheduleId(), _fireAfter, true).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(), TimeSpan.Zero) }));
        }

        [Test]
        public void Returns_schedule_lambda_decision_if_timer_is_fired_to_reschedule_an_lambda_item()
        {
            var workflow = new SingleLambdaWorkflow();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(Identity.Lambda(LambdaName).ScheduleId(), _fireAfter, TimerType.Reschedule).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName).ScheduleId(), "input") }));
        }

        [Test]
        public void Returns_schedule_child_workflow_decision_if_timer_is_fired_to_reschedule_a_child_workflow_item()
        {
            var workflow = new ChildWorkflow();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(Identity.New(WorkflowName,WorkflowVersion).ScheduleId(), _fireAfter, TimerType.Reschedule).ToArray());

            var workflowAction = workflow.Decisions(_builder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(), "input") }));
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
            var timerFiredEventGraph = _graphBuilder.TimerFiredGraph(identity.ScheduleId(), fireAfter);
            return new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }

        private TimerFiredEvent CreateRescheduleTimerFiredEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerFiredEventGraph = _graphBuilder.TimerFiredGraph(identity.ScheduleId(), fireAfter, true);
            return new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }

        private class EmptyWorkflow : Workflow
        {
        }

        private class WorkflowWithTimer : Workflow
        {
            public WorkflowWithTimer()
            {
                ScheduleTimer(TimerName);
                ScheduleLambda(LambdaName).AfterTimer(TimerName);
            }
        }
        private class WorkflowWithCompleteAction : Workflow
        {
            public WorkflowWithCompleteAction(string result)
            {
                ScheduleTimer(TimerName).OnFired(e => CompleteWorkflow(result));
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName);
            }
       }

        private class SingleLambdaWorkflow : Workflow
        {
            public SingleLambdaWorkflow()
            {
                ScheduleLambda(LambdaName);
            }
        }

        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion);
            }
        }
    }
}