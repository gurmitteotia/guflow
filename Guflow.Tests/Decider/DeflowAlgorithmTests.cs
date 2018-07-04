// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
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
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityStartedGraph = StartedActivityGraph(BookFlightActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph).Concat(bookFlightActivityStartedGraph);

            var decisions =
                new TestWorkflow().Interpret(new WorkflowHistoryEvents(allEvents,addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph);

            var decisions =
                new TestWorkflow().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.EqualTo(new[]{new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version))}));
        }



        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_just_completed_and_about_to_schedule_its_child_activity()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityStartedGraph = CompletedActivityGraph(BookFlightActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph).Concat(bookFlightActivityStartedGraph);

            var decisions =
                new TestWorkflow().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_its_parent_branches_are_not_active_because_all_items_are_completed()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph)
                    .Concat(chooseSeatActivityCompletedGraph);

            var decisions =
                new TestWorkflow().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph)
                    .Concat(chooseSeatActivityCompletedGraph);

            var decisions =
                new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchActive().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_its_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph)
                    .Concat(chooseSeatActivityCompletedGraph);

            var decisions =
                new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph);

            var decisions =
                new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchActive().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_item_when_one_of_the_not_immediate_parents_last_action_is_ignore_action_with_flag_to_keep_branch_inactive()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph);

            var decisions =
                new WorkflowHasStartingItemInBranchWithIgnoreActionToKeepBranchInactive().Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Does_not_schedule_the_child_item_when_one_of_its_parent_branch_is_active_because_it_is_reexecuting()
        {
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var reebokFlightActicityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
         
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(reebokFlightActicityCompletedGraph)
                    .Concat(chooseSeatActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph);
            var workflow = new WorkflowHasImmediateParentWithIgnoreActionToKeepBranchInactive();

            var decisions = workflow.Interpret(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId));

            Assert.That(decisions, Is.Empty);
        }

        [Test] // joint workflow item = item with multiple parents
        public void Jumping_down_in_branch_after_the_joint_item_triggers_scheduling_of_joint_item_when_its_all_parent_branches_are_inactive()
        {
            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithJumpToChildBranch();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);
            
            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new []
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)), 
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version))
            }));
        }

        [Test] 
        public void Does_not_trigger_scheduling_of_joint_item_when_jumping_without_trigger()
        {
            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithAJumpToChildBranchWithoutTrigger();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version))
            }));
        }

        [Test]
        public void Jumping_before_joint_item_does_not_trigger_scheduling_of_joint_item()
        {
            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithJumpToParentBranch();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChooseSeatActivity, Version)), 
            }));
        }

        [Test]
        public void Jumping_on_to_joint_item_does_not_trigger_duplicate_scheduling_of_joint_item()
        {
            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithJumpToFirstMultiParentChild();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)), 
            }));
        }

        [Test]
        public void Jump_down_after_joint_item_does_not_trigger_scheduling_of_joint_item_when_other_branch_is_active()
        {
            var bookHotel = StartedActivityGraph(BookHotelActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithJumpToChildBranch();
            var allEvents = bookFlight.Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version)), 
            }));

        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_activity()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new WorkflowWithFalseSchedulableCondition();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)), 
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableCondition();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Return_a_custom_action_when_scheduling_condition_is_evaluated_to_false()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new TriggerJointOnFalseSchedulableCondition();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result"), 
            }));
        }

        [Test]
        public void By_default_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new WorkflowWithFalseSchedulableConditionForTimer();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Manually_trigger_first_joint_item_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new WorkflowManuallyTriggerJointOnFalseSchedulableConditionForTimer();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)),
            }));
        }

        [Test]
        public void Return_custom_action_when_scheduling_condition_is_evaluated_to_false_for_timer()
        {

            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);

            var workflow = new ManuallyTriggerCustomActionOnFalseSchedulableConditionForTimer();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);

            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new CompleteWorkflowDecision("result")
            }));
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_all_of_its_startup_activities_are_not_scheduled()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var workflow = new WorkflowWithNotSchedulableStartupActivities();
            var historyEvents = new WorkflowHistoryEvents(new []{startedEvent});
            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void No_workflow_item_is_scheduled_when_its_startup_activity_and_timer_are_not_scheduled()
        {
            var startedEvent = _builder.WorkflowStartedEvent();
            var workflow = new WorkflowWithNotSchedulableStartupActivityAndTimer();
            var historyEvents = new WorkflowHistoryEvents(new[] { startedEvent });
            var decisions = workflow.Interpret(historyEvents);

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Schedule_first_joint_item_when_the_branch_is_made_inactive_by_ignore_action()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents = chooseSeatActivityCompletedGraph.Concat(bookFlightActivityCompletedGraph)
                    .Concat(addDinnerActivity)
                    .Concat(bookHotelActivityCompletedGraph);

            var decisions =
                new WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreAction().Interpret(new WorkflowHistoryEvents(allEvents,
                    chooseSeatActivityCompletedGraph.Last().EventId, chooseSeatActivityCompletedGraph.First().EventId));

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
        }

        [Test]
        public void Can_override_the_trigger_action_when_a_branch_is_made_inactive_by_ignore_action()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents = chooseSeatActivityCompletedGraph.Concat(bookFlightActivityCompletedGraph)
                .Concat(addDinnerActivity)
                .Concat(bookHotelActivityCompletedGraph);

            var decisions =
                new WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreActionAndOverridingTriggerAction().Interpret(new WorkflowHistoryEvents(allEvents,
                    chooseSeatActivityCompletedGraph.Last().EventId, chooseSeatActivityCompletedGraph.First().EventId));

            Assert.That(decisions, Is.Empty);
        }


        private IEnumerable<HistoryEvent> CompletedActivityGraph(string activityName)
        {
            return _builder.ActivityCompletedGraph(Identity.New(activityName, Version), "id", "result");
        }
        private IEnumerable<HistoryEvent> StartedActivityGraph(string activityName)
        {
            return _builder.ActivityStartedGraph(Identity.New(activityName, Version), "id");
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
                ScheduleActivity(ChooseSeatActivity, Version).When(a=>false, a=>Trigger(a).FirstJoint()).AfterActivity(BookFlightActivity, Version);

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
                ScheduleTimer(TimerName).When(a => false, a=>Trigger(a).FirstJoint()).AfterActivity(BookFlightActivity, Version);

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

                ScheduleActivity(BookFlightActivity, Version).When(_=>false);
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
        private class WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreAction : Workflow
        {
            public WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreAction()
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity,Version)
                    .AfterActivity(BookFlightActivity, Version)
                    .OnCompletion(a => Ignore.MakeBranchInactive());

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity,Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreActionAndOverridingTriggerAction : Workflow
        {
            public WorkflowWithOneOfBranchIsBecomingInActiveByIgnoreActionAndOverridingTriggerAction()
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
    }
}