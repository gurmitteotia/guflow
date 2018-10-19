// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerScheduleTests
    {
        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "Timer1";

        private const string LambdaName = "LambdaName";
        private const string ParentTimerName = "Timer2";

        private const string ChildWorkflowName = "Cname";
        private const string ChildWorkflowVersion = "1.0";

        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }

        [Test]
        public void Timer_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new TimerAfterActivityWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(0)) }));
        }

        [Test]
        public void Timer_can_be_scheduled_after_timer()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new TimerAfterTimerWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(0)) }));
        }

        [Test]
        public void Timer_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new TimerAfterLambdaWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(0)) }));
        }


        [Test]
        public void Timer_can_be_scheduled_after_child_workflow()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new TimerAfterChildWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(0)) }));
        }


        [Test]
        public void Timer_can_be_scheduled_after_child_workflow_using_generic_type_api()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new TimerAfterChildWorkflowUsingGenericApi().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleTimerDecision(Identity.Timer(TimerName), TimeSpan.FromSeconds(0)) }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion), "id",
                "res").ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .TimerFiredGraph(Identity.Timer(ParentTimerName), TimeSpan.FromSeconds(2))
                .ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName), "input", "result").ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents ChildWorkflowCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ChildWorkflowCompletedGraph(Identity.New(ChildWorkflowName, ChildWorkflowVersion), "rid", "input",
                    "result")
                .ToArray());
            return _eventsBuilder.Result();

        }
        private class TimerAfterActivityWorkflow : Workflow
        {
            public TimerAfterActivityWorkflow()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
                ScheduleTimer(TimerName).AfterActivity(ActivityName, ActivityVersion);
            }
        }
        private class TimerAfterTimerWorkflow : Workflow
        {
            public TimerAfterTimerWorkflow()
            {
                ScheduleTimer(ParentTimerName);
                ScheduleTimer(TimerName).AfterTimer(ParentTimerName);
            }
        }
        private class TimerAfterLambdaWorkflow : Workflow
        {
            public TimerAfterLambdaWorkflow()
            {
                ScheduleLambda(LambdaName);
                ScheduleTimer(TimerName).AfterLambda(LambdaName);
            }
        }

        private class TimerAfterChildWorkflow : Workflow
        {
            public TimerAfterChildWorkflow()
            {
                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion);
                ScheduleTimer(TimerName).AfterChildWorkflow(ChildWorkflowName, ChildWorkflowVersion);
            }
        }

        private class TimerAfterChildWorkflowUsingGenericApi : Workflow
        {
            public TimerAfterChildWorkflowUsingGenericApi()
            {
                ScheduleChildWorkflow<ChildWorkflow>();
                ScheduleTimer(TimerName).AfterChildWorkflow<ChildWorkflow>();
            }
        }

        [WorkflowDescription(ChildWorkflowVersion, Name=ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {
        }
    }
}