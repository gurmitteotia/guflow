using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class StartWorkflowActionTests
    {
        [Test]
        public void Equality_tests()
        {
            var workflowItem = new Mock<IWorkflowItems>();
            Assert.True(WorkflowAction.StartWorkflow(workflowItem.Object).Equals(WorkflowAction.StartWorkflow(workflowItem.Object)));
            Assert.False(WorkflowAction.StartWorkflow(workflowItem.Object).Equals(WorkflowAction.StartWorkflow(new Mock<IWorkflowItems>().Object)));
        }

        [Test]
        public void Return_workflow_completed_decision_when_workflow_does_not_have_any_schedulable_items()
        {
            var emptyWorkflow = new EmptyWorkflow();
            var startWorkflowAction = WorkflowAction.StartWorkflow(emptyWorkflow);

            var startupDecisions = startWorkflowAction.GetDecisions();

            CollectionAssert.AreEqual(startupDecisions, new[] { new CompleteWorkflowDecision("Workflow is completed as no schedulable item is found.") });
        }

        [Test]
        public void Return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();
            var startWorkflowAction = WorkflowAction.StartWorkflow(workflow);

            var workflowStartedDecisions = startWorkflowAction.GetDecisions();

            Assert.That(workflowStartedDecisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Download", "1.0")) }));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new WorkflowReturningStartWorkflowAction();
            var activityCompletedEvent = CreateCompletedActivityEvent(WorkflowReturningStartWorkflowAction.ActivityName, WorkflowReturningStartWorkflowAction.ActivityVersion);

            var workflowAction = activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.StartWorkflow(workflow)));
        }
        private ActivityCompletedEvent CreateCompletedActivityEvent(string activityName, string activityVersion)
        {
            var allHistoryEvents = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New(activityName, activityVersion), "id", "res");
            return new ActivityCompletedEvent(allHistoryEvents.First(), allHistoryEvents);
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleActivity("Download", "1.0");

                ScheduleActivity("Transcode", "2.0").After("Download", "1.0");
            }
        }
        private class WorkflowReturningStartWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public WorkflowReturningStartWorkflowAction()
            {
                ScheduleActivity(ActivityName, ActivityVersion).OnCompletion(e=>StartWorkflow());
            }
        }
    }
}