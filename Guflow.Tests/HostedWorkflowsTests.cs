using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class HostedWorkflowsTests
    {
        [Test]
        public void Returns_matching_hosted_workflow_by_name_and_version()
        {
            var hostedWorkflow1 = new TestWorkflow1();
            var hostedWorkflow2 = new TestWorkflow2();
            var hostedWorkflows = new HostedWorkflows(new Workflow[]{hostedWorkflow1,hostedWorkflow2});

            Assert.That(hostedWorkflows.FindBy("TestWorkflow1","2.0"),Is.EqualTo(hostedWorkflow1));
            Assert.That(hostedWorkflows.FindBy("TestWorkflow2", "1.0"), Is.EqualTo(hostedWorkflow2));
        }

        [Test]
        public void Throws_exception_when_hosted_workflow_is_not_found()
        {
            var hostedWorkflow = new TestWorkflow1();
            var hostedWorkflows = new HostedWorkflows(new [] { hostedWorkflow });

            Assert.Throws<WorkflowNotHostedException>(()=>hostedWorkflows.FindBy("TestWorkflow2", "2.0"));
        }

        [Test]
        public void Throws_exception_when_same_workflow_is_hosted_twice()
        {
            var hostedWorkflow1 = new TestWorkflow1();
            var hostedWorkflow2 = new TestWorkflow1();
            Assert.Throws<WorkflowAlreadyHostedException>(()=> new HostedWorkflows(new Workflow[] { hostedWorkflow1, hostedWorkflow2 }));
        }

        [WorkflowDescription("2.0")]
        private class TestWorkflow1 : Workflow
        {
        }
        [WorkflowDescription("1.0")]
        private class TestWorkflow2 : Workflow
        {
        }
    }
}