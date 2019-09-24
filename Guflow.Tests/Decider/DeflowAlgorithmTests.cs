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
        private const string BookHotelDinnerLambda = "AddDinnerLambda";
        private const string BookFlightLambda = "BookFlightLambda";
        private const string ChooseSeatLambda = "ChooseSeatLambda";
        private const string ChargeCustomerLambda = "ChargeCustomerLambda";
        private const string SendEmailLambda = "SendEmailLambda";
        private const string BookAirportTaxyLambda = "BookAirportTaxyLambda";
        private const string BookSubwayTicketLambda = "BookSubwayTicketLambda";

        private const string InvoiceCustomerWorkflow = "InvoiceCustomer";
        private const string ChooseSeatWorkflow = "ChoosSeat";
        private const string BookFlightWorkflow = "BookFlightWorkflow";

        private EventGraphBuilder _graph;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _graph = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graph.WorkflowStartedEvent());
        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityStartedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active_by_a_reschedule_timer()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(TimerStartedGraph(Identity.New(BookFlightActivity,Version),true));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()) }));
        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_just_completed_and_about_to_schedule_its_child_activity()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_its_parent_branches_are_not_active_because_all_items_are_completed()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new TestWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchActive().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));

            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive()
                    .Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchActive().Decisions(
                    _builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var decisions = new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchInactive().Decisions(
                    _builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_parent_branch_is_active_because_it_is_reexecuting()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(ChooseSeatActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));

            _builder.AddNewEvents(ActivityCompletedGraph(AddDinnerActivity));

            var workflow = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test] // joint workflow item = item with multiple parents
        public void Jumping_down_in_branch_after_the_joint_item_triggers_scheduling_of_joint_item_when_its_all_parent_branches_are_inactive()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));

            var workflow = new WorkflowWithJumpToChildBranch();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version).ScheduleId())
            }));
        }

        [Test]
        public void Does_not_trigger_scheduling_of_joint_item_when_jumping_without_trigger()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithAJumpToChildBranchWithoutTrigger();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version).ScheduleId())
            }));
        }

        [Test]
        public void Jumping_before_joint_item_does_not_trigger_scheduling_of_joint_item()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToParentBranch();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChooseSeatActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Jumping_on_to_joint_item_does_not_trigger_duplicate_scheduling_of_joint_item()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToFirstMultiParentChild();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Jump_down_after_joint_item_does_not_trigger_scheduling_of_joint_item_when_other_branch_is_active()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithJumpToChildBranch();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_activity()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithFalseSchedulableCondition();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableCondition();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Return_a_custom_action_when_scheduling_condition_is_evaluated_to_false()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new TriggerJointOnFalseSchedulableCondition();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result"),
            }));
        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowWithFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Return_custom_action_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(BookFlightActivity));
            var workflow = new ManuallyTriggerCustomActionOnFalseSchedulableConditionForTimer();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result")
            }));
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_all_of_its_startup_activities_are_not_scheduled()
        {
            _builder.AddNewEvents(_graph.WorkflowStartedEvent());
            var workflow = new WorkflowWithNotSchedulableStartupActivities();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_its_startup_activity_and_timer_are_not_scheduled()
        {
            _builder.AddNewEvents(_graph.WorkflowStartedEvent());
            var workflow = new WorkflowWithNotSchedulableStartupActivityAndTimer();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_custom_actions_when_the_startup_items_cannot_be_scheduled_becauses_of_false_when_clause()
        {
            _builder.AddNewEvents(_graph.WorkflowStartedEvent());
            var workflow = new CustomActionOnNonSchedulableStartupItem();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new []
            {
                new RecordMarkerWorkflowDecision("marker1", "1"), 
                new RecordMarkerWorkflowDecision("marker2", "2"), 
                new RecordMarkerWorkflowDecision("marker3", "3"), 
                new RecordMarkerWorkflowDecision("marker4", "4"), 
            }));
        }

        [Test]
        public void Schedule_first_joint_item_when_the_branch_is_made_inactive_by_ignore_action()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));
          
            var decisions = new WorkflowWithBranchBecomingInActiveByIgnoreAction().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version).ScheduleId()) }));
        }

        [Test]
        public void Can_override_the_trigger_action_when_a_branch_is_made_inactive_by_ignore_action()
        {
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookHotelActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(AddDinnerActivity));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));
           
            var decisions = new WorkflowWithBranchBecomingInActiveAndOverridingTriggerAction().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }


        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_lambda_in_its_parent_branch_is_active()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaStartedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(ActivityCompletedGraph(ChooseSeatActivity));

            var decisions = new LambdaWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_lambda_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            _builder.AddProcessedEvents(_graph.WorkflowStartedEvent());
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new LambdaWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input") }));
        }

        [Test]
        public void Schedule_first_joint_lambda_when_jumping_down_the_branch()
        {
            _builder.AddProcessedEvents(_graph.WorkflowStartedEvent());
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new JumpToLambdaWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input"),
                new ScheduleLambdaDecision(Identity.Lambda(SendEmailLambda).ScheduleId(), "input"), 
            }));
        }

        [Test]
        public void Schedule_the_first_join_lambda_when_schedule_condition_is_evalauated_to_false_for_not_startup_item()
        {
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graph.WorkflowStartedEvent(new {ChooseSeat = false}));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new LambdaWithConditionWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input"),
            }));
        }

        [Test]
        public void Return_empty_decision_when_schedule_condition_is_evalauated_to_false_for_startup_item()
        {
            _builder = new HistoryEventsBuilder();
            _builder.AddNewEvents(_graph.WorkflowStartedEvent(new { BookFlight=false }));

            var decisions = new LambdaWithConditionWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(BookHotelLambda).ScheduleId(), "input"),
            }));
        }

        [Test]
        public void Override_the_on_false_trigger_action_to_return_custom_action()
        {
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graph.WorkflowStartedEvent(new { ChooseSeat = false }));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new WorkflowWithCustomTrigger().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("finished"), 
            }));
        }

        [Test]
        public void Schedule_the_first_joint_item_when_jumping_down_to_child_workflow()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(ChooseSeatLambda));

            var decisions = new WorkflowWithJumpToChildWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input"),
                new ScheduleChildWorkflowDecision(Identity.New(InvoiceCustomerWorkflow, Version).ScheduleId(),"input"), 
            }));

        }

        [Test]
        public void Trigger_the_scheduling_of_first_joint_item_when_child_workflow_when_clause_evaluated_to_false()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new ChildWorkflowWithFalseWhen().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input"),
            }));

        }

        [Test]
        public void Does_not_trigger_the_scheduling_of_first_joint_item_when_child_workflow_is_a_startup_item_and_its_when_clause_is_evaluated_to_false()
        {
            _builder = new HistoryEventsBuilder();
            _builder.AddNewEvents(_graph.WorkflowStartedEvent("input"));

            var decisions = new StartupChildWorkflowWithFalseWhen().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(BookHotelLambda).ScheduleId(), "input"),
            }));

        }

        [Test]
        public void Provide_custom_action_when_child_workflows_when_clause_evaluated_to_false()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookFlightLambda));

            var decisions = new ChildWorkflowWithCustomActionOnFalseWhen(WorkflowAction.CompleteWorkflow("result")).Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []
            {
                new CompleteWorkflowDecision("result") 
            }));

        }

        [Test]
        public void Does_not_schedule_joint_item_when_one_of_the_branch_remains_active_by_jumping_to_parent_lamdba()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(ChooseSeatLambda));

            var decisions = new JumpToParentLambdaToKeepBranchActive().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(BookFlightLambda).ScheduleId(), "input"),
            }));
        }

        [Test]
        public void Does_not_schedule_joint_item_when_one_of_the_branch_remains_active_by_jumping_to_parent_timer()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(ChooseSeatLambda));

            var decisions = new JumpToParentTimerToKeepBranchActive().Decisions(_builder.Result()).ToArray();

            var scheduleId = Identity.Timer(TimerName).ScheduleId();
            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(scheduleId, TimeSpan.Zero);
        }

        [Test]
        public void Does_not_schedule_joint_item_when_one_of_the_branch_remains_active_by_jumping_to_parent_child_workflow()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(ChildWorkflowCompletedGraph(BookFlightWorkflow, Version));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(ChooseSeatLambda));

            var decisions = new JumpToParentChildWorkflowToKeepBranchActive().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleChildWorkflowDecision(Identity.New(BookFlightWorkflow, Version).ScheduleId(), "input"), 
            }));
        }

        [Test]
        public void Does_not_schedule_joint_item_when_one_of_the_branch_remains_active_by_jumping_to_parent_activity()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(ActivityCompletedGraph(BookFlightActivity));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(ChooseSeatLambda));

            var decisions = new JumpToParentActivityToKeepBranchActive().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []
            {
                new ScheduleActivityDecision(Identity.New(BookFlightActivity, Version).ScheduleId()),
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_becomes_inactive_on_false_when_clause()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveOnFalseWhenClause().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Does_not_schedule_joint_item_when_one_of_the_branch_remains_active_by_waiting_for_signals()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddNewEvents(bf);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new ActiveBranchBecauseOfWaitSignalAction().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]
            {
                new WaitForSignalsDecision(new WaitForSignalData{ScheduleId = Identity.Lambda(BookFlightLambda).ScheduleId(), TriggerEventId = bf.First().EventId})
            }));
        }

        [Test]
        public void Does_not_schedule_joint_item_when_the_immediate_parent_of_other_branch_remains_active_by_waiting_for_signals()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            var cs = LambdaCompletedGraph(ChooseSeatLambda);
            _builder.AddNewEvents(cs);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new ImmediateParentWaitForSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]
            {
                new WaitForSignalsDecision(new WaitForSignalData{ScheduleId = Identity.Lambda(ChooseSeatLambda).ScheduleId(), TriggerEventId = cs.First().EventId})
            }));
        }

        [Test]
        public void Schedule_joint_item_when_current_item_is_continued_by_signal_and_other_branch_is_inactive()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            var cs = LambdaCompletedGraph(ChooseSeatLambda);
            _builder.AddNewEvents(cs);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));
            var s = _graph.WorkflowSignaledEvent("SeatConfirmed", "");
            _builder.AddNewEvents(s);

            var decisions = new ImmediateParentWaitForSignal().Decisions(_builder.Result());

            var csId = Identity.Lambda(ChooseSeatLambda).ScheduleId();
            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(new WaitForSignalData{ScheduleId = csId, TriggerEventId = cs.First().EventId}),
                new WorkflowItemSignalledDecision(csId, cs.First().EventId,"SeatConfirmed", s.EventId),
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_ignore_workflow_action_after_getting_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            var sg = _graph.WaitForSignalEvent(bfId, bf.First().EventId, new []{"FlightConfirmed"}, SignalWaitType.Any);
            _builder.AddProcessedEvents(sg);
            var s = _graph.WorkflowSignaledEvent("FlightConfirmed", "");
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingIgnoreActionAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new WorkflowItemSignalledDecision(bfId, bf.First().EventId, "FlightConfirmed", s.EventId),
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_ignore_workflow_action_after_composite_signal_action()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graph.WorkflowSignaledEvent("FlightConfirmed", ""));
            _builder.AddProcessedEvents(_graph.WorkflowItemSignalledEvent(bfId, bf.First().EventId, "FlightConfirmed"));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingIgnoreActionAfterCompositeSignalAction().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_composite_ignore_workflow_action_after_signal_action()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graph.WorkflowSignaledEvent("FlightConfirmed", ""));
            _builder.AddProcessedEvents(_graph.WorkflowItemSignalledEvent(bfId, bf.First().EventId, "FlightConfirmed"));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingCompositeIgnoreActionAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Does_not_schedule_the_joint_item_when_other_branch_remains_active_by_rescheduling_the_item_on_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            var cs = LambdaCompletedGraph(ChooseSeatLambda);
            _builder.AddProcessedEvents(cs);
            var csId = Identity.Lambda(ChooseSeatLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(csId, cs.First().EventId, new[] { "SeatConfirmed" }, SignalWaitType.Any, SignalNextAction.Reschedule));
            var s = _graph.WorkflowSignaledEvent("SeatConfirmed", "");
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new ImmediateParentWaitRescheduleOnSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new WorkflowItemSignalledDecision(csId, cs.First().EventId, "SeatConfirmed", s.EventId),
                new ScheduleLambdaDecision(csId, "input"), 
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_jump_workflow_action_after_getting_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            var sg = _graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any);
            _builder.AddProcessedEvents(sg);
            var s = _graph.WorkflowSignaledEvent("FlightConfirmed", "");
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingJumpActionAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(SendEmailLambda).ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(bfId, bf.First().EventId, "FlightConfirmed", s.EventId),
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_ignore_workflow_action_in_when_clause_of_activity_after_getting_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graph.WorkflowSignaledEvent("FlightConfirmed", ""));
            _builder.AddProcessedEvents(_graph.WorkflowItemSignalledEvent(bfId, bf.First().EventId, "FlightConfirmed"));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingIgnoreActionForActivityAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_ignore_workflow_action_in_when_clause_of_timer_after_getting_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graph.WorkflowSignaledEvent("FlightConfirmed", ""));
            _builder.AddProcessedEvents(_graph.WorkflowItemSignalledEvent(bfId, bf.First().EventId, "FlightConfirmed"));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingIgnoreActionFoTimerAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []
            {
                new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")
            }));
        }

        [Test]
        public void Schedule_joint_item_when_other_branch_has_become_inactive_using_ignore_workflow_action_in_when_clause_of_child_workflow_after_getting_signal()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            var bf = LambdaCompletedGraph(BookFlightLambda);
            _builder.AddProcessedEvents(bf);
            var bfId = Identity.Lambda(BookFlightLambda).ScheduleId();
            _builder.AddProcessedEvents(_graph.WaitForSignalEvent(bfId, bf.First().EventId, new[] { "FlightConfirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graph.WorkflowSignaledEvent("FlightConfirmed", ""));
            _builder.AddProcessedEvents(_graph.WorkflowItemSignalledEvent(bfId, bf.First().EventId, "FlightConfirmed"));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new BranchBecomeInactiveUsingIgnoreActionForChildWorkflowAfterSignal().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(ChargeCustomerLambda).ScheduleId(), "input")}));
        }

        [Test]
        public void Schedule_only_one_of_the_child_of_multiple_parents()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelLambda));

            var decisions = new OneOfTheChildOfMultipleParentsDoesNotScheduleWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda(BookHotelDinnerLambda).ScheduleId(), "input")}));
        }

        [Test]
        public void Schedule_only_one_of_two_children_of_multiple_parents_and_children_have_joint_item()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelLambda));

            var decisions = new OneOfTheChildOfMultipleParentsDoesNotScheduleAndItHasChildJointItem().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(BookHotelDinnerLambda).ScheduleId(), "input") }));
        }

        [Test]
        public void Schedule_only_one_of_three_children_of_multiple_parents_and_children_have_joint_item()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelLambda));

            var decisions = new TwoOfTheChildrenOfMultipleParentsDoesNotScheduleAndTheyHaveChildJointItem().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(BookHotelDinnerLambda).ScheduleId(), "input") }));
        }

        [Test]
        public void Schedule_two_of_three_children_of_multiple_parents_and_children_have_joint_item()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelLambda));

            var decisions = new TwoOfTheChildrenOfMultipleParentsScheduleAndTheyHaveChildJointItem().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]
            {
                new ScheduleLambdaDecision(Identity.Lambda(BookHotelDinnerLambda).ScheduleId(), "input"),
                new ScheduleLambdaDecision(Identity.Lambda(BookAirportTaxyLambda).ScheduleId(), "input"),

            }));
        }

        [Test]
        public void Schedule_only_one_of_two_children_of_multiple_parents_when_one_parent_branch_is_inacive_and_children_have_joint_item()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var decisions = new OneParentBranchBecomeInactiveAndOnlyOneOfTheChildrenIsScheduled().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(BookAirportTaxyLambda).ScheduleId(), "input") }));
        }

        [Test]
        public void Reset_the_continued_item_states_after_execution()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var workflow = new OneOfTheChildrenOfMultipleParentsThrowsExceptionAndTheyHaveChildJointItem() {Limit = 2};
            Assert.Throws<InvalidOperationException>(() => workflow.Decisions(_builder.Result()));
            workflow.Limit = 20;
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleLambdaDecision(Identity.Lambda(BookAirportTaxyLambda).ScheduleId(), "input") }));
        }

        [Test]
        public void Does_not_schedule_children_when_other_parent_branch_is_active_jumping_up_to_parents_in_false_when_expression()
        {
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookFlightLambda));
            _builder.AddProcessedEvents(LambdaCompletedGraph(BookHotelLambda));
            _builder.AddNewEvents(LambdaCompletedGraph(BookHotelDinnerLambda));

            var workflow = new InOneParentBranchJumpToParentOnFalseWhenClause(); 
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        private HistoryEvent[] ActivityCompletedGraph(string activityName)
        {
            return _graph.ActivityCompletedGraph(Identity.New(activityName, Version).ScheduleId(), "id", "result").ToArray();
        }
        private HistoryEvent[] ActivityStartedGraph(string activityName)
        {
            return _graph.ActivityStartedGraph(Identity.New(activityName, Version).ScheduleId(), "id").ToArray();
        }
        private HistoryEvent[] LambdaCompletedGraph(string lambdaName)
        {
            return _graph.LambdaCompletedEventGraph(Identity.Lambda(lambdaName).ScheduleId(), "id", "result").ToArray();
        }
        private HistoryEvent[] LambdaStartedGraph(string lambdaName)
        {
            return _graph.LambdaStartedEventGraph(Identity.Lambda(lambdaName).ScheduleId(), "id").ToArray();
        }

        private HistoryEvent[] TimerStartedGraph(Identity identity, bool isARescheduleTimer)
        {
            return _graph.TimerStartedGraph(identity.ScheduleId(), TimeSpan.Zero, isARescheduleTimer).ToArray();
        }

        private HistoryEvent[] ChildWorkflowCompletedGraph(string name, string version)
        {
            return _graph.ChildWorkflowCompletedGraph(Identity.New(name, version).ScheduleId(), "id","input" ,"result").ToArray();
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
        public class CustomActionOnNonSchedulableStartupItem : Workflow
        {
            public CustomActionOnNonSchedulableStartupItem()
            {
                ScheduleActivity(BookHotelActivity, Version).When(_ => false, _=>RecordMarker("marker1", "1"));
                ScheduleTimer(TimerName).When(_ => false, _ => RecordMarker("marker2", "2"));
                ScheduleLambda(BookHotelLambda).When(_ => false, _ => RecordMarker("marker3", "3"));
                ScheduleChildWorkflow(BookFlightWorkflow,Version).When(_ => false, _ => RecordMarker("marker4", "4"));
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
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class JumpToLambdaWorkflow : Workflow
        {
            public JumpToLambdaWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda)
                    .OnCompletion(e => Jump.ToLambda(SendEmailLambda));

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterActivity(ChooseSeatActivity, Version);

                ScheduleLambda(SendEmailLambda).AfterLambda(ChargeCustomerLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class LambdaWithConditionWorkflow : Workflow
        {
            public LambdaWithConditionWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).When(_ => Input.BookFlight);
                ScheduleLambda(ChooseSeatLambda).When(_ => Input.ChooseSeat).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithCustomTrigger : Workflow
        {
            public WorkflowWithCustomTrigger()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).When(_ => Input.BookFlight);
                ScheduleLambda(ChooseSeatLambda).When(_ => Input.ChooseSeat, _=>CompleteWorkflow("finished")).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithJumpToChildWorkflow : Workflow
        {
            public WorkflowWithJumpToChildWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda)
                    .OnCompletion(_ => Jump.ToChildWorkflow(InvoiceCustomerWorkflow, Version));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);

                ScheduleChildWorkflow(InvoiceCustomerWorkflow, Version).AfterLambda(ChargeCustomerLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class ChildWorkflowWithFalseWhen : Workflow
        {
            public ChildWorkflowWithFalseWhen()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleChildWorkflow(ChooseSeatWorkflow,Version).When(_ => false).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda)
                    .AfterChildWorkflow(ChooseSeatWorkflow, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class ChildWorkflowWithCustomActionOnFalseWhen : Workflow
        {
            public ChildWorkflowWithCustomActionOnFalseWhen(WorkflowAction action)
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleChildWorkflow(ChooseSeatWorkflow, Version).When(_ => false, _=>action).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda)
                    .AfterChildWorkflow(ChooseSeatWorkflow, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class StartupChildWorkflowWithFalseWhen : Workflow
        {
            public StartupChildWorkflowWithFalseWhen()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleChildWorkflow(BookFlightWorkflow, Version).When(_ => false);
                ScheduleChildWorkflow(ChooseSeatWorkflow, Version).AfterChildWorkflow(BookFlightWorkflow,Version);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda)
                    .AfterChildWorkflow(ChooseSeatWorkflow, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class JumpToParentLambdaToKeepBranchActive : Workflow
        {
            public JumpToParentLambdaToKeepBranchActive()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda)
                    .OnCompletion(_ => Jump.ToLambda(BookFlightLambda));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class JumpToParentTimerToKeepBranchActive : Workflow
        {
            public JumpToParentTimerToKeepBranchActive()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleTimer(TimerName).AfterLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterTimer(TimerName)
                    .OnCompletion(_ => Jump.ToTimer(TimerName));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        [WorkflowDescription("1.0")]
        private class JumpToParentChildWorkflowToKeepBranchActive : Workflow
        {
            public JumpToParentChildWorkflowToKeepBranchActive()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleChildWorkflow(BookFlightWorkflow, Version);
                ScheduleLambda(ChooseSeatLambda).AfterChildWorkflow(BookFlightWorkflow, Version)
                    .OnCompletion(_ => Jump.ToChildWorkflow(BookFlightWorkflow, Version));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }


        [WorkflowDescription("1.0")]
        private class JumpToParentActivityToKeepBranchActive : Workflow
        {
            public JumpToParentActivityToKeepBranchActive()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleLambda(ChooseSeatLambda).AfterActivity(BookFlightActivity, Version)
                    .OnCompletion(_ => Jump.ToActivity(BookFlightActivity, Version));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }
        private class BranchBecomeInactiveOnFalseWhenClause : Workflow
        {
            public BranchBecomeInactiveOnFalseWhenClause()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class ActiveBranchBecauseOfWaitSignalAction : Workflow
        {
            public ActiveBranchBecauseOfWaitSignalAction()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class ImmediateParentWaitForSignal : Workflow
        {
            public ImmediateParentWaitForSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("SeatConfirmed"));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }
        private class BranchBecomeInactiveUsingIgnoreActionAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingIgnoreActionAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _=>Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class BranchBecomeInactiveUsingIgnoreActionAfterCompositeSignalAction : Workflow
        {
            public BranchBecomeInactiveUsingIgnoreActionAfterCompositeSignalAction()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed")+RecordMarker("marker", "detail"));
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class BranchBecomeInactiveUsingCompositeIgnoreActionAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingCompositeIgnoreActionAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive() + RecordMarker("marker", "m"));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class BranchBecomeInactiveUsingJumpActionAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingJumpActionAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _ => Jump.ToLambda(SendEmailLambda));

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
                ScheduleLambda(SendEmailLambda).AfterLambda(ChargeCustomerLambda);
            }
        }

        private class ImmediateParentWaitRescheduleOnSignal : Workflow
        {
            public ImmediateParentWaitRescheduleOnSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("SeatConfirmed").ToReschedule());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class BranchBecomeInactiveUsingIgnoreActionForActivityAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingIgnoreActionForActivityAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleActivity(ChooseSeatActivity,Version).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        private class BranchBecomeInactiveUsingIgnoreActionFoTimerAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingIgnoreActionFoTimerAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleTimer(TimerName).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterTimer(TimerName);
            }
        }

        private class BranchBecomeInactiveUsingIgnoreActionForChildWorkflowAfterSignal : Workflow
        {
            public BranchBecomeInactiveUsingIgnoreActionForChildWorkflowAfterSignal()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda).OnCompletion(e => e.WaitForSignal("FlightConfirmed"));
                ScheduleChildWorkflow(ChooseSeatWorkflow, Version).AfterLambda(BookFlightLambda).When(_ => false, _ => Ignore.MakeBranchInactive());

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterChildWorkflow(ChooseSeatWorkflow, Version);
            }
        }

        private class OneOfTheChildOfMultipleParentsDoesNotScheduleWorkflow : Workflow
        {
            public OneOfTheChildOfMultipleParentsDoesNotScheduleWorkflow()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookFlightLambda);

                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda).When(_ => false);
            }
        }

        private class OneOfTheChildOfMultipleParentsDoesNotScheduleAndItHasChildJointItem : Workflow
        {
            public OneOfTheChildOfMultipleParentsDoesNotScheduleAndItHasChildJointItem()
            {
                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(BookHotelLambda);

                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda).When(_ => false);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
            }
        }

        private class TwoOfTheChildrenOfMultipleParentsDoesNotScheduleAndTheyHaveChildJointItem : Workflow
        {
            public TwoOfTheChildrenOfMultipleParentsDoesNotScheduleAndTheyHaveChildJointItem()
            {
                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(BookHotelLambda);

                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda).When(_ => false);
                ScheduleLambda(BookAirportTaxyLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda).When(_ => false);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda)
                    .AfterLambda(BookAirportTaxyLambda);
            }
        }

        private class TwoOfTheChildrenOfMultipleParentsScheduleAndTheyHaveChildJointItem : Workflow
        {
            public TwoOfTheChildrenOfMultipleParentsScheduleAndTheyHaveChildJointItem()
            {
                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(BookHotelLambda);

                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda).When(_ => false);
                ScheduleLambda(BookAirportTaxyLambda).AfterLambda(BookHotelLambda).AfterLambda(BookFlightLambda);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda)
                    .AfterLambda(BookAirportTaxyLambda);
            }
        }

        private class OneParentBranchBecomeInactiveAndOnlyOneOfTheChildrenIsScheduled : Workflow
        {
            public OneParentBranchBecomeInactiveAndOnlyOneOfTheChildrenIsScheduled()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false);

                ScheduleLambda(BookAirportTaxyLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
                ScheduleLambda(BookSubwayTicketLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda)
                    .When(_ => false);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookAirportTaxyLambda)
                    .AfterLambda(BookSubwayTicketLambda);
            }
        }

        private class OneOfTheChildrenOfMultipleParentsThrowsExceptionAndTheyHaveChildJointItem : Workflow
        {
            public int Count;
            public int Limit;
            public OneOfTheChildrenOfMultipleParentsThrowsExceptionAndTheyHaveChildJointItem()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, i => ++Count==Limit ? throw new InvalidOperationException("msg") : Trigger(i).FirstJoint());

                ScheduleLambda(BookAirportTaxyLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
                ScheduleLambda(BookSubwayTicketLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda)
                    .When(_ => false);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookAirportTaxyLambda)
                    .AfterLambda(BookSubwayTicketLambda);
            }
        }

        private class InOneParentBranchJumpToParentOnFalseWhenClause : Workflow
        {
            public InOneParentBranchJumpToParentOnFalseWhenClause()
            {
                ScheduleLambda(BookHotelLambda);
                ScheduleLambda(BookHotelDinnerLambda).AfterLambda(BookHotelLambda);

                ScheduleLambda(BookFlightLambda);
                ScheduleLambda(ChooseSeatLambda).AfterLambda(BookFlightLambda).When(_ => false, _=> Jump.ToLambda(BookFlightLambda));

                ScheduleLambda(BookAirportTaxyLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda);
                ScheduleLambda(BookSubwayTicketLambda).AfterLambda(BookHotelDinnerLambda).AfterLambda(ChooseSeatLambda)
                    .When(_ => false);

                ScheduleLambda(ChargeCustomerLambda).AfterLambda(BookAirportTaxyLambda)
                    .AfterLambda(BookSubwayTicketLambda);
            }
        }

    }
}