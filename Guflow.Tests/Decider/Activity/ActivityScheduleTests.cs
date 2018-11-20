// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityScheduleTests
    {
        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "Timer1";

        private const string LambdaName = "LambdaName";
        private const string ParentActivityName = "ParentActivity";
        private const string ParentActivityVersion = "2.0";

        private const string ChildWorkflowName = "Name";
        private const string ChildWorkflowVersion = "1.0";
        private const string ChildWorkflowPosName = "pos";
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }

        [Test]
        public void Activity_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new ActivityAfterLambdaWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_time()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new ActivityAfterTimerWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new ActivityAfterActivityWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_child_workflow()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new ActivityAfterChildWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()) }));
        }

        [Test]
        public void Activity_can_be_scheduled_after_child_workflow_using_generic_type_api()
        {
            var eventGraph = ChildWorkflowCompletedEventGraph();
            var decision = new ActivityAfterChildWorkflowGenericType().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion).ScheduleId()) }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.ActivityCompletedGraph(Identity.New(ParentActivityName, ParentActivityVersion).ScheduleId(), "id",
                "res").ToArray());
            return _eventsBuilder.Result();
        }

        private WorkflowHistoryEvents TimerCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName).ScheduleId(), TimeSpan.FromSeconds(2)).ToArray());
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
                .ChildWorkflowCompletedGraph(Identity.New(ChildWorkflowName, ChildWorkflowVersion,ChildWorkflowPosName).ScheduleId(), "rid", "input",
                    "result")
                .ToArray());
            return _eventsBuilder.Result();

        }
        private class ActivityAfterLambdaWorkflow : Workflow
        {
            public ActivityAfterLambdaWorkflow()
            {
                ScheduleLambda(LambdaName);
                ScheduleActivity(ActivityName, ActivityVersion).AfterLambda(LambdaName);
            }
        }
        private class ActivityAfterTimerWorkflow : Workflow
        {
            public ActivityAfterTimerWorkflow()
            {
                ScheduleTimer(TimerName);
                ScheduleActivity(ActivityName, ActivityVersion).AfterTimer(TimerName);
            }
        }
        private class ActivityAfterActivityWorkflow : Workflow
        {
            public ActivityAfterActivityWorkflow()
            {
                ScheduleActivity(ParentActivityName, ParentActivityVersion);
                ScheduleActivity(ActivityName, ActivityVersion)
                    .AfterActivity(ParentActivityName, ParentActivityVersion);
            }
        }

        private class ActivityAfterChildWorkflow : Workflow
        {
            public ActivityAfterChildWorkflow()
            {
                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion,ChildWorkflowPosName);
                ScheduleActivity(ActivityName, ActivityVersion)
                    .AfterChildWorkflow(ChildWorkflowName, ChildWorkflowVersion,ChildWorkflowPosName);
            }
        }

        private class ActivityAfterChildWorkflowGenericType : Workflow
        {
            public ActivityAfterChildWorkflowGenericType()
            {
                ScheduleChildWorkflow<ChildWorkflow>(ChildWorkflowPosName);
                ScheduleActivity(ActivityName, ActivityVersion)
                    .AfterChildWorkflow<ChildWorkflow>(ChildWorkflowPosName);
            }
        }

        [WorkflowDescription(ChildWorkflowVersion, Name = ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                
            }
        }
    }
}