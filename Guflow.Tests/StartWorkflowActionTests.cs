using System.Linq;
using Amazon.SimpleWorkflow.Model;
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
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(), Enumerable.Empty<HistoryEvent>());

            var startupDecisions = workflowEvent.Interpret(emptyWorkflow).GetDecisions();

            CollectionAssert.AreEqual(startupDecisions, new[] { new CompleteWorkflowDecision("Workflow completed as no schedulable item is found") });
        }

        [Test]
        public void Return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent(), Enumerable.Empty<HistoryEvent>())).GetDecisions();

            Assert.That(workflowStartedDecisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision("Download", "1.0"), }));
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
                AddActivity("Download", "1.0");

                AddActivity("Transcode", "2.0").DependsOn("Download", "1.0");
            }
        }
        private class WorkflowReturningStartWorkflowAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public WorkflowReturningStartWorkflowAction()
            {
                AddActivity(ActivityName, ActivityVersion).OnCompletion(e=>StartWorkflow());
            }
        }
    }
}