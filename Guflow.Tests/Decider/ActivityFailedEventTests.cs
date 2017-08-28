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
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _reason = "reason";
        private const string _detail = "detail";
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var failedActivityEventGraph = _builder.ActivityFailedGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _reason, _detail);
            _activityFailedEvent = new ActivityFailedEvent(failedActivityEventGraph.First(), failedActivityEventGraph);
        }
            
        [Test]
        public void Populate_event_attributes()
        {
            Assert.That(_activityFailedEvent.WorkerIdentity,Is.EqualTo(_identity));
            Assert.That(_activityFailedEvent.Reason,Is.EqualTo(_reason));
            Assert.That(_activityFailedEvent.Details,Is.EqualTo(_detail));
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

            var decisions = _activityFailedEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EqualTo(new []{new FailWorkflowDecision(_reason,_detail)}));
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
                ScheduleActivity(_activityName, _activityVersion, _positionalName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnFailure(e => workflowAction);
            }
        }
    }
}