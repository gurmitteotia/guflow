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
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
            _timerFiredEvent = CreateTimerFiredEvent(Identity.Timer(TimerName), _fireAfter);
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
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName), _fireAfter);
            Assert.Throws<IncompleteEventGraphException>(()=> new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph.Where(h=>h.EventType!=EventType.TimerStarted)));
        }

        [Test]
        public void By_default_return_continue_workflow_action()
        {
            var workflow = new WorkflowWithTimer();

            var workflowAction = _timerFiredEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.ContinueWorkflow(TimerItem.New(Identity.Timer(TimerName),null))));
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
        public void Returns_schedule_activity_decision_if_timer_is_fired_to_reschedule_an_activity_item()
        {
            var workflow = new SingleActivityWorkflow();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.New(ActivityName, ActivityVersion, PositionalName), _fireAfter, true).ToArray());

            var workflowAction = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(workflowAction, Is.EqualTo(new []{new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName)) }));
        }

        [Test]
        public void Returns_schedule_timer_decision_if_timer_is_fired_to_reschedule_a_timer_item()
        {
            var workflow = new WorkflowWithTimer();
            var rescheduleTimer = CreateRescheduleTimerFiredEvent(Identity.Timer(TimerName), _fireAfter);

            var workflowAction = rescheduleTimer.Interpret(workflow).Decisions();

            Assert.That(workflowAction, Is.EqualTo(new []{new ScheduleTimerDecision(Identity.Timer(TimerName).ScheduleId(), TimeSpan.Zero) }));
        }

        [Test]
        public void Returns_schedule_lambda_decision_if_timer_is_fired_to_reschedule_an_lambda_item()
        {
            var workflow = new SingleLambdaWorkflow();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.Lambda(LambdaName), _fireAfter, true).ToArray());

            var workflowAction = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Returns_schedule_child_workflow_decision_if_timer_is_fired_to_reschedule_a_child_workflow_item()
        {
            var workflow = new ChildWorkflow();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.New(WorkflowName,WorkflowVersion), _fireAfter, true).ToArray());

            var workflowAction = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(workflowAction, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(Identity.New(WorkflowName, WorkflowVersion), "input") }));
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
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(identity, fireAfter);
            return new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }

        private TimerFiredEvent CreateRescheduleTimerFiredEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerFiredEventGraph = _eventGraphBuilder.TimerFiredGraph(identity, fireAfter, true);
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
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(TimerName).OnFired(e => workflowAction);
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