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

            var decisions =
                new WorkflowHasImmediateParentWithIgnoreAction(false).NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.Last().EventId, addDinnerActivity.First().EventId)).Execute();

            Assert.That(decisions, Is.Empty);
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
                ScheduleActivity(AddDinnerActivity, Version).After(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).After(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).After(AddDinnerActivity, Version).After(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasImmediateParentWithIgnoreAction : Workflow
        {
            public WorkflowHasImmediateParentWithIgnoreAction(bool keepBranchActive)
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).After(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version);
                ScheduleActivity(ChooseSeatActivity, Version).After(BookFlightActivity, Version).OnCompletion(e => Ignore(keepBranchActive));

                ScheduleActivity(ChargeCustomerActivity, Version).After(AddDinnerActivity, Version).After(ChooseSeatActivity, Version);
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowHasNotImmediateParentWithIgnoreAction : Workflow
        {
            public WorkflowHasNotImmediateParentWithIgnoreAction(bool keepBranchActive)
            {
                ScheduleActivity(BookHotelActivity, Version);
                ScheduleActivity(AddDinnerActivity, Version).After(BookHotelActivity, Version);

                ScheduleActivity(BookFlightActivity, Version).OnCompletion(e => Ignore(keepBranchActive));
                ScheduleActivity(ChooseSeatActivity, Version).After(BookFlightActivity, Version);

                ScheduleActivity(ChargeCustomerActivity, Version).After(AddDinnerActivity, Version).After(ChooseSeatActivity, Version);
            }
        }
    }
}