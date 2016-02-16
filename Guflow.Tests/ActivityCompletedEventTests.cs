﻿using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivityCompletedEventTests
    {
        private const string _result = "result";
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _siblingActivityName = "Sync";
        private const string _siblingActivityVersion = "2.0";
        private ActivityCompletedEvent _activityCompletedEvent;

        [SetUp]
        public void Setup()
        {
            var completedActivityEventGraph =  HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, _identity , _result);
            _activityCompletedEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);
        }

        [Test]
        public void Populate_activity_details_from_history_events()
        {
            Assert.That(_activityCompletedEvent.Result, Is.EqualTo(_result));
            Assert.That(_activityCompletedEvent.Name, Is.EqualTo(_activityName));
            Assert.That(_activityCompletedEvent.Version, Is.EqualTo(_activityVersion));
            Assert.That(_activityCompletedEvent.PositionalName, Is.EqualTo(_positionalName));
            Assert.That(_activityCompletedEvent.Identity, Is.EqualTo(_identity));
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(()=> _activityCompletedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Return_the_scheduling_decision_for_all_child_activities()
        {
            var workflow = new WorkflowWithMultipleChilds();

            var decisions = _activityCompletedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new ScheduleActivityDecision("Transcode","2.0"),new ScheduleActivityDecision("Sync","2.1")}));
        }

        [Test]
        public void Return_empty_decision_when_no_schedulable_child_item_found()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityCompletedEvent.Interpret(workflow).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_empty_decision_when_one_of_the_sibiling_is_not_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();

            var decisions = _activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            CollectionAssert.IsEmpty(decisions);
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_all_its_parents_are_completed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName,"id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityCompletedEventGraph(_siblingActivityName,_siblingActivityVersion, "","id2", "re2"));
            var activityCompletedEvent = new ActivityCompletedEvent(allHistoryEvents.First(),allHistoryEvents);

            var decisions = activityCompletedEvent.Interpret(workflowWithMultipleParents).GetDecisions();

            Assert.That(decisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Transcode", "2.0")}));
        }

        [Test]
        public void Return_scheduling_decision_for_child_when_sibling_activity_is_failed()
        {
            var workflowWithMultipleParents = new WorkflowWithMultipleParents();
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(_activityName, _activityVersion, _positionalName, "id", "res")
                                   .Concat(HistoryEventFactory.CreateActivityFailedEventGraph(_siblingActivityName, _siblingActivityVersion, "", "id2", "re2","detail"));
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

        [Test]
        public void Return_the_custom_completion_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var interpretedAction = _activityCompletedEvent.Interpret(workflow);

            Assert.That(interpretedAction,Is.EqualTo(workflowAction.Object));
        }

        private class WorkflowWithMultipleChilds : Workflow
        {
            public WorkflowWithMultipleChilds()
            {
                AddActivity(_activityName,_activityVersion,_positionalName);

                AddActivity("Transcode", "2.0").DependsOn(_activityName,_activityVersion,_positionalName);
                AddActivity("Sync", "2.1").DependsOn(_activityName, _activityVersion, _positionalName);
            }
        }

        private class WorkflowWithMultipleParents : Workflow
        {
            public WorkflowWithMultipleParents()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
                AddActivity(_siblingActivityName, _siblingActivityVersion);
                AddActivity("Transcode", "2.0").DependsOn(_activityName, _activityVersion, _positionalName).DependsOn(_siblingActivityName,_siblingActivityVersion);
            }
        }

        private class EmptyWorkflow : Workflow
        {
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName,_activityVersion,_positionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompleted(c => workflowAction);
            }
        }
    }
}