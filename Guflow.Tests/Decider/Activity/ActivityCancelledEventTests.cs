// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityCancelledEventTests
    {
        private ActivityCancelledEvent _activityCancelledEvent;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string Identity = "machine name";
        private const string Detail = "detail";

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var scheduleId = Guflow.Decider.Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var cancelledActivityEventGraph = _builder.ActivityCancelledGraph(scheduleId, Identity, Detail);
            _activityCancelledEvent = new ActivityCancelledEvent(cancelledActivityEventGraph.First(), cancelledActivityEventGraph);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activityCancelledEvent.Details, Is.EqualTo(Detail));
            Assert.That(_activityCancelledEvent.WorkerIdentity, Is.EqualTo(Identity));
            Assert.That(_activityCancelledEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_return_cancel_workflow_decision()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityCancelledEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelWorkflowDecision(Detail)}) );
        }

        [Test]
        public void Throws_exception_when_completed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityCancelledEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomAction(workflowAction);

            var actualAction = _activityCancelledEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction));
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
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnCancelled(c => workflowAction);
            }
        }
    }
}