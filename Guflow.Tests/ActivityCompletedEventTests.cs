using System;
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Tests.TestWorkflows;
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
        private ActivityCompletedEvent _activityCompletedEvent;

        [SetUp]
        public void Setup()
        {
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _result);
            _activityCompletedEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);
        }

        [Test]
        public void Populate_activity_details_from_history_events()
        {
            Assert.That(_activityCompletedEvent.Result, Is.EqualTo(_result));
            Assert.That(_activityCompletedEvent.Name, Is.EqualTo(_activityName));
            Assert.That(_activityCompletedEvent.Version, Is.EqualTo(_activityVersion));
            Assert.That(_activityCompletedEvent.PositionalName, Is.EqualTo(_positionalName));
            Assert.That(_activityCompletedEvent.WorkerIdentity, Is.EqualTo(_identity));
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(()=> _activityCompletedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Throws_exception_when_activity_started_event_not_found_in_event_graph()
        {
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _result);
            
            Assert.Throws<IncompleteEventGraphException>(()=> new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph.Where(h=>h.EventType!=EventType.ActivityTaskStarted)));
        }
        [Test]
        public void Throws_exception_when_activity_scheduled_event_not_found_in_event_graph()
        {
            var completedActivityEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _result);

            Assert.Throws<IncompleteEventGraphException>(() => new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph.Where(h => h.EventType != EventType.ActivityTaskScheduled)));
        }

        [Test]
        public void By_default_return_continue_workflow_action()
        {
            var workflow = new SingleActivityWorkflow();

            var workflowAction = _activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(new ContinueWorkflowAction(workflow.CompletedItem,null)));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var interpretedAction = _activityCompletedEvent.Interpret(workflow);

            Assert.That(interpretedAction,Is.EqualTo(workflowAction.Object));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                CompletedItem= AddActivity(_activityName,_activityVersion,_positionalName);
            }

            public WorkflowItem CompletedItem { get; private set; }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnCompletion(c => workflowAction);
            }
        }
    }
}