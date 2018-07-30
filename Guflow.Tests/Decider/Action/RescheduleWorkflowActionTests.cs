// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RescheduleWorkflowActionTests
    {
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string LambdaName = "Lambda1";
        private const string TimerName = "TimerName1";
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }
       

        [Test]
        public void Reschedule_activity()
        {
            var workflow = new WorkflowToRescheduleActivity();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new []{ new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName))}));
        }

        [Test]
        public void Reschedule_activity_when_total_number_of_scheduling_is_less_than_allowed_limit()
        {
            var workflow = new WorkflowToRescheduleActivityUpToLimit(3);
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion,PositionalName));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion,PositionalName));
            _eventsBuilder.AddNewEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion,PositionalName));

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new []{ new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion , PositionalName))}));
        }

        [Test]
        public void Schedule_next_item_when_total_number_of_scheduling_events_exceeds_allowed_limit()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddNewEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));

            var workflow = new WorkflowToRescheduleActivityUpToLimit(3);

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("completed") }));
        }

        [Test]
        public void Reschedule_timer_when_total_number_of_activity_scheduling_is_less_than_allowed_limit()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddNewEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToRescheduleActivityWithTimerUpToLimit(2);
          
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.New(ActivityName, ActivityVersion, PositionalName), TimeSpan.FromSeconds(2), true) }));
        }

        [Test]
        public void Schedule_next_item_when_total_number_of_activity_scheduling_events_exceeds_configured_limit()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            _eventsBuilder.AddNewEvents(ActivityCompletedEventGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToRescheduleActivityWithTimerUpToLimit(2);
         
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("completed") }));
        }

        [Test]
        public void Reschedule_timer_when_total_number_of_timer_scheduling_is_less_than_allowed_limit()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, false));
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, true));
            _eventsBuilder.AddNewEvents(TimerFiredEventGraph(TimerName, false));
            var workflow = new WorkflowToRescheduleTimerWithTimerUpToLimit(2);

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(2), true) }));
        }


        [Test]
        public void Schedule_next_item_when_total_number_of_timer_scheduling_exceed_allowed_limit()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, false));
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, true));
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, false));
            _eventsBuilder.AddProcessedEvents(TimerFiredEventGraph(TimerName, true));
            _eventsBuilder.AddNewEvents(TimerFiredEventGraph(TimerName, false));
            var workflow = new WorkflowToRescheduleTimerWithTimerUpToLimit(2);

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("completed") }));
        }

        [Test]
        public void Reschedule_lambda()
        {
            var workflow = new WorkflowToRescheduleLambda();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent("input"));
            _eventsBuilder.AddNewEvents(LambdaCompletedEventGraph());
           
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Reschedule_lamdba_when_total_number_of_scheduling_is_less_than_allowed_limit()
        {
            var workflow = new WorkflowToRescheduleLambdaUpToALimit(2);
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent("input"));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedEventGraph());
            _eventsBuilder.AddNewEvents(LambdaCompletedEventGraph());
           
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Schedule_next_item_when_total_number_of_lambda_scheduled_events_exceeds_allowed_limit()
        {
            var workflow = new WorkflowToRescheduleLambdaUpToALimit(2);
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent("input"));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedEventGraph());
            _eventsBuilder.AddProcessedEvents(LambdaCompletedEventGraph());
            _eventsBuilder.AddNewEvents(LambdaCompletedEventGraph());

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion)) }));
        }

        private HistoryEvent[] LambdaCompletedEventGraph()
        {
            return _eventGraphBuilder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName), "input", "type").ToArray();
        }
        private HistoryEvent[] ActivityCompletedEventGraph(string activityName, string activityVersion, string positionalName)
        {
            return _eventGraphBuilder.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "res").ToArray();
        }
        private HistoryEvent[] TimerFiredEventGraph(string timerName, bool rescheduleTimer)
        {
            return _eventGraphBuilder.TimerFiredGraph(Identity.Timer(timerName), TimeSpan.Zero, rescheduleTimer).ToArray();
        }

        private class WorkflowToRescheduleActivity : Workflow
        {
            public WorkflowToRescheduleActivity()
            {
                ScheduleActivity(ActivityName, ActivityVersion,PositionalName).OnCompletion(Reschedule);
            }
        }

        private class WorkflowToRescheduleActivityUpToLimit : Workflow
        {
            public WorkflowToRescheduleActivityUpToLimit(uint limit)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName)
                    .OnCompletion(e => Reschedule(e).UpTo(limit));

                ScheduleAction((i)=>CompleteWorkflow("completed")).AfterActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }

        private class WorkflowToRescheduleActivityWithTimerUpToLimit : Workflow
        {
            public WorkflowToRescheduleActivityWithTimerUpToLimit(uint limit)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName)
                    .OnCompletion(e => Reschedule(e).After(TimeSpan.FromSeconds(2)).UpTo(limit));

                ScheduleAction((i)=>CompleteWorkflow("completed")).AfterActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }

        private class WorkflowToRescheduleTimerWithTimerUpToLimit : Workflow
        {
            public WorkflowToRescheduleTimerWithTimerUpToLimit(uint limit)
            {
                ScheduleTimer(TimerName).FireAfter(TimeSpan.FromSeconds(3))
                    .OnFired(e => Reschedule(e).After(TimeSpan.FromSeconds(2)).UpTo(limit));

                ScheduleAction((i) => CompleteWorkflow("completed")).AfterTimer(TimerName);
            }
        }

        private class WorkflowToRescheduleLambda : Workflow
        {
            public WorkflowToRescheduleLambda()
            {
                ScheduleLambda(LambdaName).OnCompletion(Reschedule);
            }
        }

        private class WorkflowToRescheduleLambdaUpToALimit : Workflow
        {
            public WorkflowToRescheduleLambdaUpToALimit(uint limit)
            {
                ScheduleLambda(LambdaName).OnCompletion(e => Reschedule(e).UpTo(limit));
                ScheduleActivity(ActivityName, ActivityVersion).AfterLambda(LambdaName);
            }
        }

    }
}