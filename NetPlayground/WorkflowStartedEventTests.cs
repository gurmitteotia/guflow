using System;
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
        public void Return_workflow_start_up_decision()
        {
            var workflowAction = new TestWorkflowAction();
            WorkflowReturnThisStartupDecision(workflowAction);
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(){WorkflowExecutionStartedEventAttributes = new WorkflowExecutionStartedEventAttributes()});

            var decision = workflowEvent.Interpret(_workflow.Object);

            Assert.That(decision, Is.EqualTo(workflowAction));
        }

        [Test]
        public void Workflow_is_started_with_workflow_start_arguments()
        {
            var workflowStartedAttributes = new WorkflowExecutionStartedEventAttributes() {Input = "some input"};
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(){ WorkflowExecutionStartedEventAttributes = workflowStartedAttributes});

            workflowEvent.Interpret(_workflow.Object);

            AssertThatWorkflowIsStartedFor(workflowStartedAttributes);
        }

        private void AssertThatWorkflowIsStartedFor(WorkflowExecutionStartedEventAttributes attributes)
        {
            Func<WorkflowStartedArgs, bool> match = (a) =>
            {
                Assert.AreEqual(attributes.Input, a.Input);
                return true;
            };
            _workflow.Verify(w=>w.WorkflowStarted(It.Is<WorkflowStartedArgs>(a=>match(a))),Times.Once);
        }

        private void WorkflowReturnThisStartupDecision(WorkflowAction workflowAction)
        {
            _workflow.Setup(w => w.WorkflowStarted(It.IsAny<WorkflowStartedArgs>())).Returns(workflowAction);
        }
    }
}