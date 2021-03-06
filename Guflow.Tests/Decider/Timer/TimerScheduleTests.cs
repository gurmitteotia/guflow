﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

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

        private ScheduleId _timerScheduleId;
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
            _timerScheduleId = Identity.Timer(TimerName).ScheduleId();
        }

        [Test]
        public void Timer_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decisions = new TimerAfterActivityWorkflow().Decisions(eventGraph).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(_timerScheduleId, TimeSpan.FromSeconds(0));
        }

        [Test]
        public void Timer_can_be_scheduled_after_timer()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decisions = new TimerAfterTimerWorkflow().Decisions(eventGraph).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(_timerScheduleId, TimeSpan.FromSeconds(0));
        }

        [Test]
        public void Timer_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decisions = new TimerAfterLambdaWorkflow().Decisions(eventGraph).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(_timerScheduleId, TimeSpan.FromSeconds(0));
        }


        [Test]
        public void Timer_can_be_scheduled_after_child_workflow()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decisions = new TimerAfterChildWorkflow().Decisions(eventGraph).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(_timerScheduleId, TimeSpan.FromSeconds(0));
        }


        [Test]
        public void Timer_can_be_scheduled_after_child_workflow_using_generic_type_api()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decisions = new TimerAfterChildWorkflowUsingGenericApi().Decisions(eventGraph).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(_timerScheduleId, TimeSpan.FromSeconds(0));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id",
                "res").ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .TimerFiredGraph(Identity.Timer(ParentTimerName).ScheduleId(), TimeSpan.FromSeconds(2))
                .ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName).ScheduleId(), "input", "result").ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents ChildWorkflowCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ChildWorkflowCompletedGraph(Identity.New(ChildWorkflowName, ChildWorkflowVersion).ScheduleId(), "rid", "input",
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