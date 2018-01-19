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
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _detail = "detail";

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var cancelledActivityEventGraph = _builder.ActivityCancelledGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _detail);
            _activityCancelledEvent = new ActivityCancelledEvent(cancelledActivityEventGraph.First(), cancelledActivityEventGraph);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activityCancelledEvent.Details, Is.EqualTo(_detail));
            Assert.That(_activityCancelledEvent.WorkerIdentity, Is.EqualTo(_identity));
            Assert.That(_activityCancelledEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_return_cancel_workflow_decision()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityCancelledEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelWorkflowDecision(_detail)}) );
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
                ScheduleActivity(_activityName, _activityVersion, _positionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnCancelled(c => workflowAction);
            }
        }
    }
}