// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityCompletedEventTests
    {
        private const string Result = "result";
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string Identity = "machine name";
        private const string Input = "input";
        private ActivityCompletedEvent _activityCompletedEvent;

        private EventGraphBuilder _builder;
        private ScheduleId _activityIdentity;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _activityIdentity = Guflow.Decider.Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(_activityIdentity, Identity, Result,Input);
            _activityCompletedEvent = new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph);
        }

        [Test]
        public void Populate_activity_details_from_history_events()
        {
            Assert.That(_activityCompletedEvent.Result, Is.EqualTo(Result));
            Assert.That(_activityCompletedEvent.WorkerIdentity, Is.EqualTo(Identity));
            Assert.That(_activityCompletedEvent.IsActive,Is.False);
            Assert.That(_activityCompletedEvent.Input,Is.EqualTo(Input));
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(()=> _activityCompletedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Does_not_populate_worker_id_when_activity_started_event_not_found_in_event_graph()
        {
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(_activityIdentity, Identity, Result);
            
            var activityCompletedEvent= new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph.Where(h=>h.EventType!=EventType.ActivityTaskStarted));

            Assert.That(activityCompletedEvent.WorkerIdentity,Is.Null);
        }
        [Test]
        public void Throws_exception_when_activity_scheduled_event_not_found_in_event_graph()
        {
            var completedActivityEventGraph = _builder.ActivityCompletedGraph(_activityIdentity, Identity, Result);

            Assert.Throws<IncompleteEventGraphException>(() => new ActivityCompletedEvent(completedActivityEventGraph.First(), completedActivityEventGraph.Where(h => h.EventType != EventType.ActivityTaskScheduled)));
        }

        [Test]
        public void By_default_return_continue_workflow_action()
        {
            var workflow = new SingleActivityWorkflow();

            var workflowAction = _activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.ContinueWorkflow(new ActivityItem(Guflow.Decider.Identity.New(ActivityName,ActivityVersion,PositionalName),null))));
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
                ScheduleActivity(ActivityName,ActivityVersion,PositionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCompletion(c => workflowAction);
            }
        }
    }
}