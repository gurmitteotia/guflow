// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class JumpWorkflowActionTests
    {
        private readonly Mock<IWorkflow> _workflow = new Mock<IWorkflow>();
        private const string ActivityName = "activity";
        private const string ActivityVersion = "2.0";
        private const string PositionalName = "pname";
        private const string SiblingActivityName = "BookHotelActivity";
        private const string Version = "1.0";
        private const string LambdaName = "lambda_1";
        private EventGraphBuilder _builder;
        private HistoryEventsBuilder _eventsBuilder;
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }

        [Test]
        public void Returns_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = TimerItem.New(Identity.Timer("Somename"), _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(workflowItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EquivalentTo(workflowItem.GetScheduleDecisions()));
        }

        [Test]
        public void Returns_timer_decision_when_jumped_after_a_timeout()
        {
            var workflowItem = new ActivityItem(Identity.New("name", "ver", "pos"), _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(workflowItem).After(TimeSpan.FromSeconds(2));

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleTimerDecision(Identity.New("name", "ver", "pos"), TimeSpan.FromSeconds(2), true) }));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_activity()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToReturnScheduleActivityAction();

            var workflowAction = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(workflowAction, Is.EqualTo(new []{ new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName))}));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_timer()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToReturnScheduleTimerAction();

            var workflowAction = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(workflowAction, Is.EqualTo(new []{ new ScheduleTimerDecision(Identity.Timer("SomeTimer"), TimeSpan.FromSeconds(0))}));
        }

        [Test]
        public void Jumping_out_to_different_parent_branch_is_not_allowed()
        {
            _eventsBuilder.AddNewEvents(CompletedActivityGraph(SiblingActivityName, Version));
            var workflow = new WorkflowToJumpToDifferentBranch();

            Assert.Throws<OutOfBranchJumpException>(()=> workflow.Decisions(_eventsBuilder.Result()));
        }

        [Test]
        public void Jump_to_parent_lambda_item()
        {
            _eventsBuilder.AddProcessedEvents(_builder.WorkflowStartedEvent());
            _eventsBuilder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion));

            var decisions = new WorkflowToJumpToParentLambda().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName,PositionalName),"input")}));
        }

        private class WorkflowToReturnScheduleActivityAction : Workflow
        {
            public WorkflowToReturnScheduleActivityAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => Jump.ToActivity(ActivityName, ActivityVersion, PositionalName));
            }
        }
        private class WorkflowToReturnScheduleTimerAction : Workflow
        {
            public WorkflowToReturnScheduleTimerAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => Jump.ToTimer("SomeTimer"));
                ScheduleTimer("SomeTimer").AfterActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }
     
        private HistoryEvent [] CompletedActivityGraph(string activityName, string activityVersion, string positionalName ="")
        {
            return _builder.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName), "id", "result").ToArray();
        }

        private class WorkflowToJumpToDifferentBranch : Workflow
        {
            public WorkflowToJumpToDifferentBranch()
            {
                ScheduleActivity(ActivityName, Version);

                ScheduleActivity(SiblingActivityName, Version)
                    .OnCompletion(e => Jump.ToActivity(ActivityName, Version));
            }
        }

        private class WorkflowToJumpToParentLambda : Workflow
        {
            public WorkflowToJumpToParentLambda()
            {
                ScheduleLambda(LambdaName, PositionalName);

                ScheduleActivity(ActivityName, ActivityVersion).AfterLambda(LambdaName, PositionalName)
                    .OnCompletion(e => Jump.ToLambda(LambdaName, PositionalName));
            }
        }
    }
}