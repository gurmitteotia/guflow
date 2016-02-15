using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow
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
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(_activityName, _activityVersion, _positionalName, _identity, _timeoutType, _detail); 
            _activityTimedoutEvent = new ActivityTimedoutEvent(activityTimedoutEventGraph.First(),activityTimedoutEventGraph);
        }

        [Test]
        public void Attributes_are_populated_from_history_attributes()
        {
            Assert.That(_activityTimedoutEvent.Name, Is.EqualTo(_activityName));
            Assert.That(_activityTimedoutEvent.Version, Is.EqualTo(_activityVersion));
            Assert.That(_activityTimedoutEvent.PositionalName,Is.EqualTo(_positionalName));
            Assert.That(_activityTimedoutEvent.Identity, Is.EqualTo(_identity));
            Assert.That(_activityTimedoutEvent.TimeoutType, Is.EqualTo(_timeoutType));
            Assert.That(_activityTimedoutEvent.Details,Is.EqualTo(_detail));
        }

        [Test]
        public void Throws_exception_when_activity_is_not_found_in_workflow()
        {
            var workflow = new EmptyWorkflow();

            Assert.Throws<IncompatibleWorkflowException>(() => _activityTimedoutEvent.Interpret(workflow));
        }

        [Test]
        public void Return_workflow_failed_decision()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = _activityTimedoutEvent.Interpret(workflow).GetDecisions();

            Assert.That(decisions,Is.EquivalentTo(new []{new FailWorkflowDecision(_timeoutType,_detail)}));
        }

        [Test]
        public void Can_return_the_custom_workflow_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var actualAction = _activityTimedoutEvent.Interpret(workflow);

            Assert.That(actualAction,Is.EqualTo(workflowAction.Object));
        }

        private class EmptyWorkflow : Workflow
        {
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
                AddActivity(_activityName, _activityVersion, _positionalName).OnTimedout(t => workflowAction);
            }
        }
    }
}