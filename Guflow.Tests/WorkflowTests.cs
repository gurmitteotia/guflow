using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow
{
    [TestFixture]
    public class WorkflowTests
    {
        [Test]
        public void Workflow_started_return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent(),Enumerable.Empty<HistoryEvent>())).GetDecisions();

            Assert.That(workflowStartedDecisions, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Download", "1.0"))}));
        }

        [Test]
        public void Workflow_started_return_workflow_completed_decisions_when_workflow_has_no_schedulable_items()
        {
            var workflow = new EmptyWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent(),Enumerable.Empty<HistoryEvent>())).GetDecisions();

            Assert.That(workflowStartedDecisions, Is.EquivalentTo(new[] { new CompleteWorkflowDecision("Workflow completed as no schedulable item is found")}));
        }

        [Test]
        public void On_activity_completion_return_schedule_decision_for_child_dependent_activities()
        {
            var workflow = new TestWorkflow();

            var decisionsOnActivityCompletion = workflow.ActivityCompleted(new ActivityCompletedEvent(new HistoryEvent(), Enumerable.Empty<HistoryEvent>())).GetDecisions();

            Assert.That(decisionsOnActivityCompletion, Is.EquivalentTo(new[] { new ScheduleActivityDecision(Identity.New("Transcode", "2.0"))}));
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                AddActivity("Download", "1.0");

                AddActivity("Transcode", "2.0").DependsOn("Download", "1.0");
            }
        }
    }
}