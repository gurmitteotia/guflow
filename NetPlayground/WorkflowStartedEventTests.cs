using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class WorkflowStartedEventTests
    {
        private Mock<IWorkflow> _workflow;

        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflow>();
        }

        [Test]
        public void Return_workflow_completed_decision_when_workflow_does_not_have_any_schedulable_items()
        {
            var emptyWorkflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent());

            var startupDecisions = workflowEvent.Interpret(emptyWorkflow).GetDecisions();

            Assert.That(startupDecisions.Count(),Is.EqualTo(1));
            startupDecisions.AssertThatWorkflowIsCompleted("SomeResult");
        }

        [Test]
        public void Workflow_started_return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent())).GetDecisions();

            Assert.That(workflowStartedDecisions.Count(), Is.EqualTo(1));

            workflowStartedDecisions.AssertThatActivityIsScheduled("Download", "1.0", string.Empty);
        }


        [Test]
        public void Return_workflow_start_up_decision()
        {
            var workflowAction = new TestWorkflowAction();
            WorkflowReturnThisStartupDecision(workflowAction);
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent());

            var decision = workflowEvent.Interpret(_workflow.Object);

            Assert.That(decision, Is.EqualTo(workflowAction));
        }
      
        private void WorkflowReturnThisStartupDecision(WorkflowAction workflowAction)
        {
            _workflow.Setup(w => w.WorkflowStarted(It.IsAny<WorkflowStartedEvent>())).Returns(workflowAction);
        }

        private class EmptyWorkflow : Workflow
        {
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