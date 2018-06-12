// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityCancellationFailedEventTests
    {
        private const string _activityName = "download";
        private const string _activityVersion = "1.0";
        private const string _cause = "unknown";
        private ActivityCancellationFailedEvent _activityCancellationFailedEvent;

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var historyEventGraph = _builder.ActivityCancellationFailedGraph(Identity.New(_activityName,_activityVersion),_cause);
            _activityCancellationFailedEvent = new ActivityCancellationFailedEvent(historyEventGraph.First());
        }

        [Test]
        public void Throws_exception_when_activity_is_not_found_in_workflow()
        {
           Assert.Throws<IncompatibleWorkflowException>(()=>  _activityCancellationFailedEvent.Interpret(new EmptyWorkflow()));
        }

        [Test]
        public void Should_populate_the_properties_from_event_attributes()
        {
            Assert.That(_activityCancellationFailedEvent.Cause,Is.EqualTo(_cause));
            Assert.That(_activityCancellationFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_return_fail_workflow_decision()
        {
            var decisions = _activityCancellationFailedEvent.Interpret(new TestWorkflow()).Decisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision("ACTIVITY_CANCELLATION_FAILED", _cause) }));
        }
        [Test]
        public void Can_return_custom_workflow_action()
        {
            var customAction = new Mock<WorkflowAction>().Object;
            var workflowAction = _activityCancellationFailedEvent.Interpret(new WorkflowReturnCustomAction(customAction));

            Assert.That(workflowAction,Is.EqualTo(customAction));
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleActivity(_activityName, _activityVersion);
            }
        }

        private class WorkflowReturnCustomAction : Workflow
        {
            public WorkflowReturnCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion).OnFailedCancellation(c => workflowAction);
            }
        }
    }
}