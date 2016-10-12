using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTasksTests
    {
        private Mock<IWorkflowClient> _workflowClient;
        [SetUp]
        public void Setup()
        {
            _workflowClient = new Mock<IWorkflowClient>();
        }

        [Test]
        public void Can_interpret_new_task_for_hosted_workflow()
        {
            var decisionTask = new DecisionTask()
            {
                WorkflowType = new WorkflowType() {Name = "TestWorkflow", Version = "1.0"},
                Events = new List<HistoryEvent>(),
                PreviousStartedEventId = 10,
                StartedEventId = 20,
                TaskToken = "token"
            };
            var hostedWorkflows = new HostedWorkflows(new []{new TestWorkflow()});
            var workflowTasks = new WorkflowTasks(decisionTask,_workflowClient.Object);

            workflowTasks.ExecuteFor(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient();
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient()
        {
            _workflowClient.Verify(w=>w.RespondWithDecisions("token",It.IsAny<IEnumerable<Decision>>()));

        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow:Workflow
        {
        }
    }
}