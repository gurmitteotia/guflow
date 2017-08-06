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
        private const string Version = "1.0";

        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookFlightActivityStartedGraph = StartedActivityGraph(BookFlightActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph).Concat(bookFlightActivityStartedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_of_its_parent_branches_are_not_active()
        {
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new WorkflowHasImmediateParentWithIgnoreAction(true).NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new WorkflowHasImmediateParentWithIgnoreAction(false).NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new WorkflowHasNotImmediateParentWithIgnoreAction(true).NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
                new WorkflowHasNotImmediateParentWithIgnoreAction(false).NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

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
            var workflow = new WorkflowHasImmediateParentWithIgnoreAction(false);

            var decisions = workflow.NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

            Assert.That(decisions, Is.Empty);
        }

        [Test] // joint workflow item = item with multiple parents
        public void Jumping_down_in_branch_after_the_joint_item_triggers_scheduling_of_joint_item()
        {
            var bookHotel = CompletedActivityGraph(BookHotelActivity);
            var addDinner = CompletedActivityGraph(AddDinnerActivity);
            var bookFlight = CompletedActivityGraph(BookFlightActivity);
            var workflow = new WorkflowWithJumpToChildBranch();
            var allEvents = bookFlight.Concat(addDinner).Concat(bookHotel);
            var historyEvents = new WorkflowHistoryEvents(allEvents, bookFlight.Last().EventId, bookFlight.First().EventId);
            
            var decisions = workflow.NewExecutionFor(historyEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new []
            {
                new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)), 
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

            var decisions = workflow.NewExecutionFor(historyEvents).Execute();

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

            var decisions = workflow.NewExecutionFor(historyEvents).Execute();

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

            var decisions = workflow.NewExecutionFor(historyEvents).Execute();

            Assert.That(decisions, Is.EquivalentTo(new[]
            {
                new ScheduleActivityDecision(Identity.New(SendEmailActivity, Version)), 
            }));

        }

        private static IEnumerable<HistoryEvent> CompletedActivityGraph(string activityName)
        {
            return HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, Version), "id", "result");
        }
        private static IEnumerable<HistoryEvent> StartedActivityGraph(string activityName)
        {
            return HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New(activityName, Version), "id");
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
        private class WorkflowHasImmediateParentWithIgnoreAction : Workflow
        {
            public WorkflowHasImmediateParentWithIgnoreAction(bool keepBranchActive)
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version).OnCompletion(e => Ignore(keepBranchActive));

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasNotImmediateParentWithIgnoreAction : Workflow
        {
            public WorkflowHasNotImmediateParentWithIgnoreAction(bool keepBranchActive)
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).AfterActivity(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Ignore(keepBranchActive));
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

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump(e).ToActivity(SendEmailActivity, Version));
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

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump(e).ToActivity(ChooseSeatActivity, Version));
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

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Jump(e).ToActivity(ChargeCustomerActivity, Version));
                ScheduleActivity(ChooseSeatActivity, Version).AfterActivity(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).AfterActivity(AddDinnerActivity, Version).AfterActivity(ChooseSeatActivity, Version);

                ScheduleActivity(SendEmailActivity, Version).AfterActivity(ChargeCustomerActivity, Version);
            }
        }
    }
}