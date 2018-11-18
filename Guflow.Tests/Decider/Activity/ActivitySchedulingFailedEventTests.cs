// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivitySchedulingFailedEventTests
    {
        private ActivitySchedulingFailedEvent _activitySchedulingFailedEvent;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string _cause = "detail";
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var schedulingFailedEventGraph = _builder.ActivitySchedulingFailedGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId(),_cause);
            _activitySchedulingFailedEvent = new ActivitySchedulingFailedEvent(schedulingFailedEventGraph.First());
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activitySchedulingFailedEvent.Cause, Is.EqualTo(_cause));
            Assert.That(_activitySchedulingFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_should_fail_workflow()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activitySchedulingFailedEvent.Interpret(workflow).Decisions();

            Assert.That(decisions,Is.EqualTo(new[]{new FailWorkflowDecision("ACTIVITY_SCHEDULING_FAILED",_cause)}));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var expectedAction = new Mock<WorkflowAction>().Object;
            
            var workflowAction = _activitySchedulingFailedEvent.Interpret(new WorkflowWithCustomAction(expectedAction));

            Assert.That(workflowAction,Is.EqualTo(workflowAction));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnSchedulingFailed(e => workflowAction);
            }
        }
    }
}