using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityTimedoutEventTests
    {
        private ActivityTimedoutEvent _activityTimedoutEvent;
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _identity = "machine name";
        private const string _timeoutType = "reason";
        private const string _detail = "detail";

        [SetUp]
        public void Setup()
        {
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(Identity.New(_activityName, _activityVersion, _positionalName), _identity, _timeoutType, _detail); 
            _activityTimedoutEvent = new ActivityTimedoutEvent(activityTimedoutEventGraph.First(), activityTimedoutEventGraph);
        }

        [Test]
        public void Attributes_are_populated_from_history_attributes()
        {
            Assert.That(_activityTimedoutEvent.WorkerIdentity, Is.EqualTo(_identity));
            Assert.That(_activityTimedoutEvent.TimeoutType, Is.EqualTo(_timeoutType));
            Assert.That(_activityTimedoutEvent.Details,Is.EqualTo(_detail));
            Assert.That(_activityTimedoutEvent.IsActive,Is.False);
        }

        [Test]
        public void Throws_exception_when_activity_is_not_found_in_workflow()
        {
            var workflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityTimedoutEvent.Interpret(workflow));
        }
     
        [Test]
        public void By_default_return_fail_workflow_action()
        {
            var workflow = new SingleActivityWorkflow();

            var workflowAction = _activityTimedoutEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow(_timeoutType,_detail)));
        }

        [Test]
        public void Can_return_the_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomAction(workflowAction);

            var actualAction = _activityTimedoutEvent.Interpret(workflow);

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
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnTimedout(t => workflowAction);
            }
        }
    }
}