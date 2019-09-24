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
        private const string TimerName = "SomeTimer";
        private const string ChildWorkflowName = "Cnmae";
        private const string ChildWorkflowVersion = "1.0";

        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(new []{_graphBuilder.WorkflowStartedEvent()}));
        }

        [Test]
        public void Returns_the_scheduling_decision_for_workflow_item()
        {
            var workflowItem = TimerItem.New(Identity.Timer("Somename"), _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(null, workflowItem);

            var decisions = workflowAction.Decisions();

            Assert.That(decisions, Is.EquivalentTo(workflowItem.ScheduleDecisions()));
        }

        [Test]
        public void Schedule_the_timer_with_default_scheduleid_when_it_has_not_already_scheduled()
        {
            var identity = Identity.Timer("Somename");
            var workflowItem = TimerItem.New(identity, _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(null, workflowItem);

            var decisions = workflowAction.Decisions().ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(identity.ScheduleId(), TimeSpan.Zero);
        }

        [Test]
        public void Reuse_the_scheduleid_when_it_has_already_scheduled()
        {
            const string runId = "runid";
            var identity = Identity.Timer("Somename");
            var scheduleId = identity.ScheduleId(runId + "Reset");
            _builder.AddWorkflowRunId(runId);
            _builder.AddProcessedEvents(_graphBuilder.TimerStartedGraph(scheduleId, TimeSpan.Zero).ToArray());
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());

            var workflowItem = TimerItem.New(identity, _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(null, workflowItem);

            var decisions = workflowAction.Decisions().ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(scheduleId, TimeSpan.Zero);
        }

        [Test]
        public void Returns_timer_decision_when_jumped_after_a_timeout()
        {
            var identity = Identity.New("name", "ver", "pos");
            var workflowItem = new ActivityItem(identity, _workflow.Object);
            var workflowAction = WorkflowAction.JumpTo(null, workflowItem).After(TimeSpan.FromSeconds(2));

            var decisions = workflowAction.Decisions().ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(1));
            decisions[0].AssertRescheduleTimer(identity.ScheduleId(), TimeSpan.FromSeconds(2));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_activity()
        {
            _builder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToReturnScheduleActivityAction();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{ new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId()) }));
        }

        [Test]
        public void Can_be_returned_as_workflow_action_when_scheduling_the_timer()
        {
            _builder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new WorkflowToReturnScheduleTimerAction();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(Identity.Timer("SomeTimer").ScheduleId(), TimeSpan.FromSeconds(0));
        }

        [Test]
        public void Jump_to_a_timer_ignore_its_when_clause()
        {
            _builder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion, PositionalName));
            var workflow = new JumpToTimerWithItsWhenClauseToBeAlwaysFalse();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(Identity.Timer("SomeTimer").ScheduleId(), TimeSpan.FromSeconds(0));
        }

        [Test]
        public void Jump_to_an_activity_ignore_its_when_clause()
        {
            _builder.AddNewEvents(CompletedTimerGraph(TimerName));
            var workflow = new JumpToActivityWithItsWhenClauseToBeAlwaysFalse();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId()) }));
        }

        [Test]
        public void Jump_to_an_lambda_ignore_its_when_clause()
        {
            _builder.AddNewEvents(CompletedTimerGraph(TimerName));
            var workflow = new JumpToLambdaIgnoresItsWhenClause();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(LambdaName, PositionalName).ScheduleId(), "input") }));
        }

        [Test]
        public void Jump_to_a_child_workflow_ignore_its_when_clause()
        {
            _builder.AddNewEvents(CompletedTimerGraph(TimerName));
            var workflow = new JumpToChildWorkflowIgnoresItsWhenClause();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(Identity.New(ChildWorkflowName, ChildWorkflowVersion).ScheduleId(), "input") }));
        }

        [Test]
        public void Jump_to_a_child_workflow_using_generic_api_ignore_its_when_clause()
        {
            _builder.AddNewEvents(CompletedTimerGraph(TimerName));
            var workflow = new JumpToChildWorkflowUsingGenericApiIgnoresItsWhenClause();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleChildWorkflowDecision(Identity.New(ChildWorkflowName, ChildWorkflowVersion).ScheduleId(), "input") }));
        }


        [Test]
        public void Jumping_out_to_different_parent_branch_is_not_allowed()
        {
            _builder.AddNewEvents(CompletedActivityGraph(SiblingActivityName, Version));
            var workflow = new WorkflowToJumpToDifferentBranch();

            Assert.Throws<OutOfBranchJumpException>(()=> workflow.Decisions(_builder.Result()));
        }

        [Test]
        public void Jump_to_parent_lambda_item()
        {
            _builder.AddNewEvents(CompletedActivityGraph(ActivityName, ActivityVersion));

            var decisions = new WorkflowToJumpToParentLambda().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(LambdaName,PositionalName).ScheduleId(),"input")}));
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
            return _graphBuilder.ActivityCompletedGraph(Identity.New(activityName, activityVersion, positionalName).ScheduleId(), "id", "result").ToArray();
        }

        private HistoryEvent[] CompletedTimerGraph(string timerName)
        {
            return _graphBuilder.TimerFiredGraph(Identity.Timer(timerName).ScheduleId(), TimeSpan.Zero).ToArray();
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

        private class JumpToTimerWithItsWhenClauseToBeAlwaysFalse : Workflow
        {
            public JumpToTimerWithItsWhenClauseToBeAlwaysFalse()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => Jump.ToTimer("SomeTimer"));
                ScheduleTimer(TimerName)
                    .AfterActivity(ActivityName, ActivityVersion, PositionalName)
                    .When(_ => false);
            }
        }

        private class JumpToActivityWithItsWhenClauseToBeAlwaysFalse : Workflow
        {
            public JumpToActivityWithItsWhenClauseToBeAlwaysFalse()
            {
                ScheduleTimer(TimerName)
                    .When(_ => false)
                    .OnFired(_ => Jump.ToActivity(ActivityName, ActivityVersion, PositionalName));

                ScheduleActivity(ActivityName, ActivityVersion, PositionalName)
                    .AfterTimer(TimerName)
                    .When(_ => false);
            }
        }

        private class JumpToLambdaIgnoresItsWhenClause : Workflow
        {
            public JumpToLambdaIgnoresItsWhenClause()
            {
                ScheduleTimer(TimerName)
                    .When(_ => false)
                    .OnFired(_ => Jump.ToLambda(LambdaName, PositionalName));

                ScheduleLambda(LambdaName, PositionalName)
                    .AfterTimer(TimerName)
                    .When(_ => false);
            }
        }

        private class JumpToChildWorkflowIgnoresItsWhenClause : Workflow
        {
            public JumpToChildWorkflowIgnoresItsWhenClause()
            {
                ScheduleTimer(TimerName)
                    .When(_ => false)
                    .OnFired(_ => Jump.ToChildWorkflow(ChildWorkflowName, ChildWorkflowVersion));

                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion)
                    .AfterTimer(TimerName)
                    .When(_ => false);
            }
        }

        private class JumpToChildWorkflowUsingGenericApiIgnoresItsWhenClause : Workflow
        {
            public JumpToChildWorkflowUsingGenericApiIgnoresItsWhenClause()
            {
                ScheduleTimer(TimerName)
                    .When(_ => false)
                    .OnFired(_ => Jump.ToChildWorkflow<ChildWorkflow>());

                ScheduleChildWorkflow(ChildWorkflowName, ChildWorkflowVersion)
                    .AfterTimer(TimerName)
                    .When(_ => false);
            }
        }

        [WorkflowDescription(ChildWorkflowVersion, Name = ChildWorkflowName)]
        private class ChildWorkflow : Workflow
        {

        }
    }
}