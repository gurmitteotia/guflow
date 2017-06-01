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
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var bookFlightActivityStartedGraph = StartedActivityGraph(BookFlightActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph).Concat(bookFlightActivityStartedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.First().EventId, addDinnerActivity.Last().EventId)).Execute();

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_its_parent_branches_are_not_active()
        {
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.First().EventId, addDinnerActivity.Last().EventId)).Execute();

            Assert.That(decisions, Is.EqualTo(new[]{new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version))}));
        }



        [Test]
        public void Does_not_schedule_a_child_item_when_one_of_the_activity_in_its_parent_branch_is_just_completed_and_about_to_schedule_its_child_activity()
        {
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);

            var bookFlightActivityStartedGraph = CompletedActivityGraph(BookFlightActivity);
            var allEvents = addDinnerActivity.Concat(bookHotelActivityCompletedGraph).Concat(bookFlightActivityStartedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.First().EventId, addDinnerActivity.Last().EventId)).Execute();

            Assert.That(decisions, Is.Empty);

        }

        [Test]
        public void Schedule_a_child_item_when_all_its_parent_branches_are_not_active_because_all_items_are_completed()
        {
            var addDinnerActivity = CompletedActivityGraph(AddDinnerActivity);
            var bookHotelActivityCompletedGraph = CompletedActivityGraph(BookHotelActivity);
            var bookFlightActivityCompletedGraph = CompletedActivityGraph(BookFlightActivity);
            var chooseSeatActivityCompletedGraph = CompletedActivityGraph(ChooseSeatActivity);
            var allEvents =
                addDinnerActivity.Concat(bookHotelActivityCompletedGraph)
                    .Concat(bookFlightActivityCompletedGraph)
                    .Concat(chooseSeatActivityCompletedGraph);

            var decisions =
                new TestWorkflow().NewExecutionFor(new WorkflowHistoryEvents(allEvents,
                    addDinnerActivity.First().EventId, addDinnerActivity.Last().EventId)).Execute();

            Assert.That(decisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New(ChargeCustomerActivity, Version)) }));
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
    }
}