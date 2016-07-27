using System.Linq;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivitySchedulingFailedEventTests
    {
        private ActivitySchedulingFailedEvent _activitySchedulingFailedEvent;
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _cause = "detail";

        [SetUp]
        public void Setup()
        {
            var schedulingFailedEventGraph = HistoryEventFactory.CreateActivitySchedulingFailedEventGraph(Identity.New(_activityName, _activityVersion, _positionalName),_cause);
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

            var workflowAction = _activitySchedulingFailedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.FailWorkflow("ACTIVITY_SCHEDULING_FAILED",_cause)));
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
                ScheduleActivity(_activityName, _activityVersion, _positionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleActivity(_activityName, _activityVersion, _positionalName).OnFailedScheduling(e => workflowAction);
            }
        }
    }
}