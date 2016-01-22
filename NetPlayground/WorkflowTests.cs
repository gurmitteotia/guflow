using System.Linq;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class WorkflowTests
    {
        [Test]
        public void Workflow_started_return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent())).GetDecisions();

            Assert.That(workflowStartedDecisions.Count(),Is.EqualTo(1));

            workflowStartedDecisions.AssertThatActivityIsScheduled("Download", "1.0", string.Empty);
        }

        [Test]
        public void Workflow_started_return_workflow_completed_decisions_when_workflow_has_no_schedulable_items()
        {
            var workflow = new TestEmptyWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent())).GetDecisions();

            Assert.That(workflowStartedDecisions.Count(), Is.EqualTo(1));

            workflowStartedDecisions.AssertThatActivityIsScheduled("Download", "1.0", string.Empty);
        }

        [Test]
        public void On_activity_completion_return_schedule_decision_for_child_dependent_activities()
        {
            var workflow = new TestWorkflow();

            var decisionsOnActivityCompletion = workflow.ActivityCompleted(new ActivityCompletedEvent(new HistoryEvent())).GetDecisions();

            Assert.That(decisionsOnActivityCompletion.Count(), Is.EqualTo(1));

            decisionsOnActivityCompletion.AssertThatActivityIsScheduled("Transcode", "2.0", string.Empty);
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                AddActivity("Download", "1.0");

                AddActivity("Transcode", "2.0").DependsOn("Download", "1.0");
            }
        }

        private class TestEmptyWorkflow : Workflow
        {
        }
    }
}