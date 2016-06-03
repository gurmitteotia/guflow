using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
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
        [SetUp]
        public void Setup()
        {
            var failedActivityEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _reason, _detail);
            _activityFailedEvent = new ActivityFailedEvent(failedActivityEventGraph.First(), failedActivityEventGraph);
        }
            
        [Test]
        public void Populate_event_attributes()
        {
            Assert.That(_activityFailedEvent.WorkerIdentity,Is.EqualTo(_identity));
            Assert.That(_activityFailedEvent.Reason,Is.EqualTo(_reason));
            Assert.That(_activityFailedEvent.Detail,Is.EqualTo(_detail));
        }

        [Test]
        public void Throws_exception_when_failed_activity_is_not_found_in_workflow()
        {
            var incompatibleWorkflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityFailedEvent.Interpret(incompatibleWorkflow));
        }

        [Test]
        public void By_default_return_fail_workflow_action()
        {
            var workflow = new SingleActivityWorkflow();

            var workflowAction = _activityFailedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow(_reason,_detail)));
        }
        [Test]
        public void Can_return_the_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var actualWorkflowAction = _activityFailedEvent.Interpret(workflow);

            Assert.That(actualWorkflowAction,Is.EqualTo(workflowAction.Object));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                AddActivity(_activityName, _activityVersion, _positionalName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddActivity(_activityName, _activityVersion, _positionalName).OnFailure(e => workflowAction);
            }
        }
    }
}