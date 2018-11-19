// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowScheduleTests
    {
        private const string ActivityName = "Activity1";
        private const string ActivityVersion = "1.0";
        private const string TimerName = "Timer1";

        private const string LambdaName = "LambdaName";
        private const string ParentWorkflowName = "Pname";
        private const string ParentWorkflowVersion = "1.0";

        private const string ChildWorkflowName = "Name";
        private const string ChildWorkflowVersion = "1.0";
        private const string ParentWorkflowRunId = "runid";
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;
        private ScheduleId _scheduleId;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder().AddWorkflowRunId(ParentWorkflowRunId);
            _scheduleId = Identity.New(ChildWorkflowName, ChildWorkflowVersion).ScheduleId(ParentWorkflowRunId);
        }

        [Test]
        public void Child_workflow_can_be_scheduled_after_lambda()
        {
            var eventGraph = LambdaCompletedEventGraph();
            var decision = new ChildWorkflowAfterLambdaWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }

        [Test]
        public void Child_workflow_can_be_scheduled_after_timer()
        {
            var eventGraph = TimerCompletedEventGraph();
            var decision = new ChildWorkflowAfterTimerWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }

        [Test]
        public void Child_workflow_can_be_scheduled_after_child_workflow()
        {
            var eventGraph = ParentWorkflowCompletedEventGraph();
            var decision = new ChildWorkflowAfterChildWorkflow().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }

        [Test]
        public void Child_workflow_can_be_scheduled_after_activity()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new ChildWorkflowAfterActivity().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }


        [Test]
        public void Child_workflow_can_be_scheduled_after_activity_using_generic_api()
        {
            var eventGraph = ActivityEventGraph();
            var decision = new ChildWorkflowAfterActivityUsingGenericApi().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }

        [Test]
        public void Child_workflow_can_be_scheduled_after_child_workflow_using_generic_type_api()
        {
            var eventGraph = ParentWorkflowCompletedEventGraph();
            var decision = new ChildWorkflowAfterChildWorkflowGenericType().Decisions(eventGraph);

            Assert.That(decision, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(_scheduleId, "input") }));
        }

        private WorkflowHistoryEvents ActivityEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.ActivityCompletedGraph(Identity.New(ActivityName, ActivityVersion).ScheduleId(), "id","res").ToArray());
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
        private WorkflowHistoryEvents ParentWorkflowCompletedEventGraph()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ChildWorkflowCompletedGraph(Identity.New(ParentWorkflowName, ParentWorkflowVersion).ScheduleId(ParentWorkflowRunId), "rid", "input",
                    "result")
                .ToArray());
            return _eventsBuilder.Result();

        }
        private class ChildWorkflowAfterLambdaWorkflow : Workflow
        {
            public ChildWorkflowAfterLambdaWorkflow()
            {
                ScheduleLambda(LambdaName);
                ScheduleChildWorkflow<ChildWorkflow>().AfterLambda(LambdaName);
            }
        }
        private class ChildWorkflowAfterTimerWorkflow : Workflow
        {
            public ChildWorkflowAfterTimerWorkflow()
            {
                ScheduleTimer(TimerName);
                ScheduleChildWorkflow<ChildWorkflow>().AfterTimer(TimerName);
            }
        }
        private class ChildWorkflowAfterChildWorkflow : Workflow
        {
            public ChildWorkflowAfterChildWorkflow()
            {
                ScheduleChildWorkflow(ParentWorkflowName, ParentWorkflowVersion);
                ScheduleChildWorkflow<ChildWorkflow>()
                    .AfterChildWorkflow(ParentWorkflowName, ParentWorkflowVersion);
            }
        }

        private class ChildWorkflowAfterActivity : Workflow
        {
            public ChildWorkflowAfterActivity()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
                ScheduleChildWorkflow<ChildWorkflow>().AfterActivity(ActivityName, ActivityVersion);
            }
        }

        private class ChildWorkflowAfterActivityUsingGenericApi : Workflow
        {
            public ChildWorkflowAfterActivityUsingGenericApi()
            {
                ScheduleActivity<TestActivity>();
                ScheduleChildWorkflow<ChildWorkflow>().AfterActivity<TestActivity>();
            }
        }

        [ActivityDescription(ActivityVersion, Name = ActivityName)]
        private class TestActivity : Activity
        {

        }

        private class ChildWorkflowAfterChildWorkflowGenericType : Workflow
        {
            public ChildWorkflowAfterChildWorkflowGenericType()
            {
                ScheduleChildWorkflow<ParentWorkflow>();
                ScheduleChildWorkflow<ChildWorkflow>()
                    .AfterChildWorkflow<ParentWorkflow>();
            }
        }

        [WorkflowDescription(ChildWorkflowVersion, Name = ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {

            }
        }

        [WorkflowDescription(ParentWorkflowVersion, Name = ParentWorkflowName)]
        private class ParentWorkflow : Workflow
        {
            public ParentWorkflow()
            {

            }
        }
    }
}