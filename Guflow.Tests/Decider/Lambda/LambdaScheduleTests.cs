// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaScheduleTests
    {
        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "Timer1";

        private const string LambdaName = "LambdaName";
        private const string ParentLambdaName = "ParentLambda";
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Lambda_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new LambdaAfterActivityWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName),"input")}));
        }

        [Test]
        public void Lambda_can_be_scheduled_after_timer()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new LambdaAfterTimerWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Lambda_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new LambdaAfterLambdaWorkflow().Interpret(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion), "id",
                "res");
            return new WorkflowHistoryEvents(completedEvent.Concat(new []{startedEvent}), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.TimerFiredGraph(Identity.Timer(TimerName), TimeSpan.FromSeconds(2));
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var completedEvent = _builder.LambdaCompletedEventGraph(Identity.Lambda(ParentLambdaName),"input", "result", "cont");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }
        private class LambdaAfterActivityWorkflow : Workflow
        {
            public LambdaAfterActivityWorkflow()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
                ScheduleLambda(LambdaName).AfterActivity(ActivityName, ActivityVersion);
            }
        }
        private class LambdaAfterTimerWorkflow : Workflow
        {
            public LambdaAfterTimerWorkflow()
            {
                ScheduleTimer(TimerName);
                ScheduleLambda(LambdaName).AfterTimer(TimerName);
            }
        }
        private class LambdaAfterLambdaWorkflow : Workflow
        {
            public LambdaAfterLambdaWorkflow()
            {
                ScheduleLambda(ParentLambdaName);
                ScheduleLambda(LambdaName).AfterLambda(ParentLambdaName);
            }
        }
    }
}