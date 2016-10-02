using System.Threading.Tasks;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowHostTests
    {
        private WorkflowHost _workflowHost;
        private WorkflowClient _workflowClient;

        [SetUp]
        public void Setup()
        {
            _workflowHost = _workflowClient.CreateHostFor(new TestWorkflow());
        }
        [Test]
        public async Task Execute_the_workflow_task_when_opened()
        {
            await _workflowHost.Open();
        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow : Workflow
        {
        }
    }
}