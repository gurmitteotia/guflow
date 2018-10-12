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

        private const string ChildWorkflowName = "CWorkflow";
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
        public void Lambda_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new LambdaAfterActivityWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName),"input")}));
        }

        [Test]
        public void Lambda_can_be_scheduled_after_timer()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new LambdaAfterTimerWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Lambda_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new LambdaAfterLambdaWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }


        [Test]
        public void Lambda_can_be_scheduled_after_child_workflow()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new LambdaAfterChildWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        [Test]
        public void Lambda_can_be_scheduled_after_child_workflow_using_generic_type_api()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new LambdaAfterChildWorkflowUsingGenericApi().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName), "input") }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            var startedEvent = _eventGraphBuilder.WorkflowStartedEvent();
            var completedEvent = _eventGraphBuilder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion), "id",
                "res");
            return new WorkflowHistoryEvents(completedEvent.Concat(new []{startedEvent}), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            var startedEvent = _eventGraphBuilder.WorkflowStartedEvent();
            var completedEvent = _eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName), TimeSpan.FromSeconds(2));
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
        }

        private WorkflowHistoryEvents LambdaCompletedEventGraph()
        {
            var startedEvent = _eventGraphBuilder.WorkflowStartedEvent();
            var completedEvent = _eventGraphBuilder.LambdaCompletedEventGraph(Identity.Lambda(ParentLambdaName),"input", "result");
            return new WorkflowHistoryEvents(completedEvent.Concat(new[] { startedEvent }), completedEvent.Last().EventId, completedEvent.First().EventId);
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

        private class LambdaAfterChildWorkflow : Workflow
        {
            public LambdaAfterChildWorkflow()
            {
                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion);

                ScheduleLambda(LambdaName).AfterChildWorkflow(ChildWorkflowName, ChildWorkflowVersion);
            }
        }

        private class LambdaAfterChildWorkflowUsingGenericApi : Workflow
        {
            public LambdaAfterChildWorkflowUsingGenericApi()
            {
                ScheduleChildWorkflow<ChildWorkflow>();

                ScheduleLambda(LambdaName).AfterChildWorkflow<ChildWorkflow>();
            }
        }

        [WorkflowDescription(ChildWorkflowVersion, Name = ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {

        }
    }
}