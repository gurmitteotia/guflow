using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class StartWorkflowActionTests
    {
        [Test]
        public void Return_workflow_completed_decision_when_workflow_does_not_have_any_schedulable_items()
        {
            var emptyWorkflow = new EmptyWorkflow();
            var startWorkflowAction = emptyWorkflow.StartupAction;

            var startupDecisions = startWorkflowAction.GetDecisions();

            CollectionAssert.AreEqual(startupDecisions, new[] { new CompleteWorkflowDecision("Workflow is completed because no schedulable item was found.") });
        }

        [Test]
        public void Return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();
            var startWorkflowAction = workflow.StartupAction;

            var workflowStartedDecisions = startWorkflowAction.GetDecisions();

            Assert.That(workflowStartedDecisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Download", "1.0")) }));
        }

        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflow = new WorkflowReturningStartWorkflowAction();
            var activityCompletedEvent = CreateCompletedActivityEvent(WorkflowReturningStartWorkflowAction.ActivityName, WorkflowReturningStartWorkflowAction.ActivityVersion);

            var workflowAction = activityCompletedEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(workflow.StartupAction));
        }

        [Test]
        public void Returns_the_decisions_for_startup_schedulable_workflow_action()
        {
            var workflow = new WorkflowToScheduleAction("reason", "detailss");
            var startWorkflowAction = workflow.StartupAction;

            var action = startWorkflowAction.GetDecisions();

            Assert.That(action, Is.EqualTo(new []{new FailWorkflowDecision("reason", "detailss")}));
        }

        private class WorkflowToScheduleAction : Workflow
        {
            public WorkflowToScheduleAction(string reason, string details)
            {
                ScheduleAction((i)=>FailWorkflow(reason, details));
            }
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

                ScheduleActivity("Transcode", "2.0").AfterActivity("Download", "1.0");
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