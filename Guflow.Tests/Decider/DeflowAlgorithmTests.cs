// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class DeflowAlgorithmTests
    {
        private const string BookHotelActivity = "BookHotelActivity";
        private const string AddDinnerActivity = "AddDinnerActivity";
        private const string BookFlightActivity = "BookFlightActivity";
        private const string ChooseSeatActivity = "ChooseSeatActivity";
        private const string ChargeCustomerActivity = "ChargeCustomerActivity";
        private const string SendEmailActivity = "SendEmailActivity";
        private const string TimerName = "DelayTimer";
        private const string Version = "1.0";
        private const string BookHotelLambda = "BookHotel";
        private const string AddDinnerLambda = "AddDinnerLambda";
        private const string BookFlightLambda = "BookFlightLambda";
        private const string ChooseSeatLambda = "ChooseSeatLambda";
        private const string ChargeCustomerLambda = "ChargeCustomerLambda";
        private const string SendEmailLambda = "SendEmailLambda";

        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityStartedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active_by_a_reschedule_timer()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(TimerStartedGraph(Identity.New(BookFlightActivity,Version),true));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_just_completed_and_about_to_schedule_its_child_activity()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_its_parent_branches_are_not_active_because_all_items_are_completed()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchActive().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive()
                    .Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchActive().Decisions(
                    _eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchInactive().Decisions(
                    _eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_parent_branch_is_active_because_it_is_reexecuting()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));

            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var workflow = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test] // joint workflow item = item with multiple parents
        public void Jumping_down_in_branch_after_the_joint_item_triggers_scheduling_of_joint_item_when_its_all_parent_branches_are_inactive()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));

            var workflow = new WorkflowWithJumpToChildBranch();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version))
            }));
        }

        [Test]
        public void Does_not_trigger_scheduling_of_joint_item_when_jumping_without_trigger()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithAJumpToChildBranchWithoutTrigger();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version))
            }));
        }

        [Test]
        public void Jumping_before_joint_item_does_not_trigger_scheduling_of_joint_item()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToParentBranch();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChooseSeatActivity, Version)),
            }));
        }

        [Test]
        public void Jumping_on_to_joint_item_does_not_trigger_duplicate_scheduling_of_joint_item()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToFirstMultiParentChild();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Jump_down_after_joint_item_does_not_trigger_scheduling_of_joint_item_when_other_branch_is_active()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToChildBranch();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version)),
            }));
        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_activity()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithFalseSchedulableCondition();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableCondition();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Return_a_custom_action_when_scheduling_condition_is_evaluated_to_false()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new TriggerJointOnFalseSchedulableCondition();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result"),
            }));
        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Return_custom_action_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new ManuallyTriggerCustomActionOnFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result")
            }));
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_all_of_its_startup_activities_are_not_scheduled()
        {
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowStartedEvent());
            var workflow = new WorkflowWithNotSchedulableStartupActivities();

            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_its_startup_activity_and_timer_are_not_scheduled()
        {
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowStartedEvent());
            var workflow = new WorkflowWithNotSchedulableStartupActivityAndTimer();
            var decisions = workflow.Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_first_joint_item_when_the_branch_is_made_inactive_by_ignore_action()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));
          
            var decisions = new WorkflowWithBranchBecomingInActiveByIgnoreAction().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Can_override_the_trigger_action_when_a_branch_is_made_inactive_by_ignore_action()
        {
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _eventsBuilder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));
           
            var decisions = new WorkflowWithBranchBecomingInActiveAndOverridingTriggerAction().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);
        }


        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_lambda_in_its_parent_branch_is_active()
        {
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _eventsBuilder.AddProcessedEvents(LambdaStartedGraph(AddDinnerLambda));
            _eventsBuilder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));

            var decisions = new LambdaWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_lambda_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _eventsBuilder.AddNewEvents(LambdaCompletedGraph(AddDinnerLambda));

            var decisions = new LambdaWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda), "input") }));
        }

        [Test]
        public void Schedule_first_joint_lambda_when_jumping_down_the_branch()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _eventsBuilder.AddNewEvents(LambdaCompletedGraph(AddDinnerLambda));

            var decisions = new JumpToLambdaWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda), "input"),
                new ScheduleLambdaDecision(Identity.Lambda(SendEmailLambda), "input"), 
            }));
        }

        [Test]
        public void Schedule_the_first_join_lambda_when_schedule_condition_is_evalauated_to_false_for_not_startup_item()
        {
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent(new {ChooseSeat = false}));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(AddDinnerLambda));
            _eventsBuilder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new LambdaWithConditionWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda), "input"),
            }));
        }

        [Test]
        public void Return_empty_decision_when_schedule_condition_is_evalauated_to_false_for_startup_item()
        {
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddNewEvents(_eventGraphBuilder.WorkflowStartedEvent(new { BookFlight=false }));

            var decisions = new LambdaWithConditionWorkflow().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(BookHotelLambda), "input"),
            }));
        }

        [Test]
        public void Override_the_on_false_trigger_action_to_return_custom_action()
        {
            _eventsBuilder = new HistoryEventsBuilder();
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent(new { ChooseSeat = false }));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _eventsBuilder.AddProcessedEvents(LambdaCompletedGraph(AddDinnerLambda));
            _eventsBuilder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new WorkflowWithCustomTrigger().Decisions(_eventsBuilder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("finished"), 
            }));
        }

        private HistoryEvent[] ActivityCompletedGraph(string activityName)
        {
            return _eventGraphBuilder.ActivityCompletedGraph(Identity.New(activityName, Version), "id", "result").ToArray();
        }
        private HistoryEvent[] ActivityStartedGraph(string activityName)
        {
            return _eventGraphBuilder.ActivityStartedGraph(Identity.New(activityName, Version), "id").ToArray();
        }
        private HistoryEvent[] LambdaCompletedGraph(string lambdaName)
        {
            return _eventGraphBuilder.LambdaCompletedEventGraph(Identity.Lambda(lambdaName), "id", "result").ToArray();
        }
        private HistoryEvent[] LambdaStartedGraph(string lambdaName)
        {
            return _eventGraphBuilder.LambdaStartedEventGraph(Identity.Lambda(lambdaName), "id").ToArray();
        }

        private HistoryEvent[] TimerStartedGraph(Identity identity, bool isARescheduleTimer)
        {
            return _eventGraphBuilder.TimerStartedGraph(identity, TimeSpan.Zero, isARescheduleTimer).ToArray();
        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive : Workflow
        {
            public WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version).OnCompletion(e => Ignore.MakeBranchInactive());

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasImmediateParentWithIgnoreActionToKeepBranchActive : Workflow
        {
            public WorkflowHasImmediateParentWithIgnoreActionToKeepBranchActive()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version).OnCompletion(e => Ignore);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchActive : Workflow
        {
            public WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchActive()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Ignore);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchInactive : Workflow
        {
            public WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchInactive()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Ignore.MakeBranchInactive());
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithJumpToChildBranch : Workflow
        {
            public WorkflowWithJumpToChildBranch()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump.ToActivity(SendEmailActivity, Version));
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithAJumpToChildBranchWithoutTrigger : Workflow
        {
            public WorkflowWithAJumpToChildBranchWithoutTrigger()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump.ToActivity(SendEmailActivity, Version).WithoutTrigger());
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithJumpToParentBranch : Workflow
        {
            public WorkflowWithJumpToParentBranch()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump.ToActivity(ChooseSeatActivity, Version));
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithJumpToFirstMultiParentChild : Workflow
        {
            public WorkflowWithJumpToFirstMultiParentChild()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump.ToActivity(ChargeCustomerActivity, Version));
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithFalseSchedulableCondition : Workflow
        {
            public WorkflowWithFalseSchedulableCondition()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).When(a => false).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }
        [WorkflowDescription("1.0")]
        private class WorkflowManuallyTriggerJointOnFalseSchedulableCondition : Workflow
        {
            public WorkflowManuallyTriggerJointOnFalseSchedulableCondition()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).When(a => false, a => Trigger(a).FirstJoint()).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class TriggerJointOnFalseSchedulableCondition : Workflow
        {
            public TriggerJointOnFalseSchedulableCondition()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).When(a => false, a => CompleteWorkflow("result")).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithFalseSchedulableConditionForTimer : Workflow
        {
            public WorkflowWithFalseSchedulableConditionForTimer()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleTimer(TimerName).When(a => false).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterTimer(TimerName);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowManuallyTriggerJointOnFalseSchedulableConditionForTimer : Workflow
        {
            public WorkflowManuallyTriggerJointOnFalseSchedulableConditionForTimer()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleTimer(TimerName).When(a => false, a => Trigger(a).FirstJoint()).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterTimer(TimerName);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class ManuallyTriggerCustomActionOnFalseSchedulableConditionForTimer : Workflow
        {
            public ManuallyTriggerCustomActionOnFalseSchedulableConditionForTimer()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleTimer(TimerName).When(a => false, a => CompleteWorkflow("result")).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterTimer(TimerName);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        public class WorkflowWithNotSchedulableStartupActivities : Workflow
        {
            public WorkflowWithNotSchedulableStartupActivities()
            {
                ScheduleActivity(BookHotelActivity, Version).When(_ => false);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).When(_ => false);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        public class WorkflowWithNotSchedulableStartupActivityAndTimer : Workflow
        {
            public WorkflowWithNotSchedulableStartupActivityAndTimer()
            {
                ScheduleActivity(BookHotelActivity, Version).When(_ => false);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleTimer(TimerName).When(_ => false);
                ScheduleActivity(ChooseSeatActivity, Version).AfterTimer(TimerName);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }


        [WorkflowDescription("1.0")]
        private class WorkflowWithBranchBecomingInActiveByIgnoreAction : Workflow
        {
            public WorkflowWithBranchBecomingInActiveByIgnoreAction()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version)
                    .AfterActivity(BookFlightActivity, Version)
                    .OnCompletion(a => Ignore.MakeBranchInactive());

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithBranchBecomingInActiveAndOverridingTriggerAction : Workflow
        {
            public WorkflowWithBranchBecomingInActiveAndOverridingTriggerAction()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version)
                    .AfterActivity(BookFlightActivity, Version)
                    .OnCompletion(a => Ignore.MakeBranchInactive(Ignore));

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class LambdaWorkflow : Workflow
        {
            public LambdaWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(AddDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(AddDinnerLambda).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class JumpToLambdaWorkflow : Workflow
        {
            public JumpToLambdaWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(AddDinnerLambda).AfterLambda(BookHotelLambda)
                    .OnCompletion(e => Jump.ToLambda(SendEmailLambda));

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(AddDinnerLambda).AfterActivity(ChooseSeatActivity, Version);

                ScheduleLambda(SendEmailLambda).AfterLambda(ChargeCustomerLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class LambdaWithConditionWorkflow : Workflow
        {
            public LambdaWithConditionWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(AddDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).When(_ => Input.BookFlight);
                ScheduleLambda(ChooseSeatLambda).When(_ => Input.ChooseSeat).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(AddDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithCustomTrigger : Workflow
        {
            public WorkflowWithCustomTrigger()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(AddDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).When(_ => Input.BookFlight);
                ScheduleLambda(ChooseSeatLambda).When(_ => Input.ChooseSeat, _=>CompleteWorkflow("finished")).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(AddDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }
    }
}