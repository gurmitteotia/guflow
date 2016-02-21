using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ContinueWorkflowActionTests
    {
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _siblingActivityName = "Sync";
        private const string _siblingActivityVersion = "2.0";

        [Test]
        public void Return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var completedWorkflowItem = new ActivityItem(_activityName,_activityVersion,_positionalName,workflow);
            var completedActivityEventsGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res");
            var workflowAction = new ContinueWorkflowAction(completedWorkflowItem, new WorkflowContext(completedActivityEventsGraph));
            
            var decisions = workflowAction.GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0"), new ScheduleActivityDecision("Sync", "2.1") }));
        }

        [Test]
        public void Return_the_scheduling_decision_for_all_child_activities1()
        {
            var workflow = new WorkflowWithMultipleChilds();
            var completedActivityEventsGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res");
            var activityCompletedEvent = new ActivityCompletedEvent(completedActivityEventsGraph.First(),completedActivityEventsGraph);
            var decisions = activityCompletedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0"), new ScheduleActivityDecision("Sync", "2.1") }));
        }

        [Test]
        public void Return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();
            var activityCompletedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);
            var decisions = activityCompletedEvent.Interpret(workflow).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_empty_decision_when_one_of_the_sibiling_is_not_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var activityCompletedEvent = CreateCompletedActivityEvent(_activityName, _activityVersion, _positionalName);
           
            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_all_its_parents_are_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_sibling_activity_is_failed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityFailedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_sibling_activity_is_timedout()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityTimedoutEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_sibling_activity_is_cancelled()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCancelledEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "detail"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0") }));
        }

        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion, string positionalName)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(activityName, activityVersion, positionalName, "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }


        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                CompletedItem =
                    AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(Continue);

                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName);
                AddActivity("Sync", "2.1").DependsOn(_activityName, _activityVersion, _positionalName);
            }

            public WorkflowItem CompletedItem { get; private set; }
        }

        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
                AddActivity(_siblingActivityName, _siblingActivityVersion);
                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName).DependsOn(_siblingActivityName, _siblingActivityVersion);
            }
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                CompletedItem = AddActivity(_activityName, _activityVersion, _positionalName);
            }

            public WorkflowItem CompletedItem { get; private set; }
        }
    }
}