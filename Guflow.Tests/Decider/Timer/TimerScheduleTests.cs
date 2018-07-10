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
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
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

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion), "id",
                "res");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.TimerFiredGraph(Identity.Timer(ParentTimerName), TimeSpan.FromSeconds(2));
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName), "input", "result", "cont");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
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
    }
}