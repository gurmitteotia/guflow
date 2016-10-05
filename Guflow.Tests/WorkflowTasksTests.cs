using System.Threading;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTasksTests
    {
        private IWorkflowClient _workflowClient;
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflow;
        [SetUp]
        public void Setup()
        {
            _amazonWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _workflowClient = new WorkflowClient(_amazonWorkflow.Object);
        }

        [Test]
        public void Can_interpret_new_task_for_hosted_workflow()
        {
            var decisionTask = new DecisionTask();
            var hostedWorkflows = new HostedWorkflows();
            var workflowTasks = new WorkflowTasks(decisionTask);

            workflowTasks.ExecuteFor(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient();
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient()
        {
            _amazonWorkflow.Verify(a=>a.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(),It.IsAny<CancellationToken>())).
                            
        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow:Workflow
        {
        }
    }
}