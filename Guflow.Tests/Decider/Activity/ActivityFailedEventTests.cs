// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityFailedEventTests
    {
        private ActivityFailedEvent _activityFailedEvent;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string Identity = "machine name";
        private const string Reason = "reason";
        private const string Detail = "detail";
        private EventGraphBuilder _builder;
        private ScheduleId _scheduleId;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _scheduleId = Guflow.Decider.Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var failedActivityEventGraph = _builder.ActivityFailedGraph(_scheduleId, Identity, Reason, Detail);
            _activityFailedEvent = new ActivityFailedEvent(failedActivityEventGraph.First(), failedActivityEventGraph);
        }
            
        [Test]
        public void Populate_event_attributes()
        {
            Assert.That(_activityFailedEvent.WorkerIdentity,Is.EqualTo(Identity));
            Assert.That(_activityFailedEvent.Reason,Is.EqualTo(Reason));
            Assert.That(_activityFailedEvent.Details,Is.EqualTo(Detail));
            Assert.That(_activityFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void Throws_exception_when_failed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityFailedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void By_default_return_fail_workflow_decision()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityFailedEvent.Interpret(workflow).Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision(Reason,Detail)}));
        }
        [Test]
        public void Can_return_the_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomAction(workflowAction);

            var actualWorkflowAction = _activityFailedEvent.Interpret(workflow);

            Assert.That(actualWorkflowAction,Is.EqualTo(workflowAction));
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
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnFailure(e => workflowAction);
            }
        }
    }
}