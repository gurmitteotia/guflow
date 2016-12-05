using System;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class StartWorkflowRequestTests
    {
        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new StartWorkflowRequest("", "1.0", "id"));
            Assert.Throws<ArgumentException>(() => new StartWorkflowRequest("name", "", "id"));
            Assert.Throws<ArgumentException>(() => new StartWorkflowRequest("name", "1.0", ""));
            Assert.Throws<ArgumentException>(() => StartWorkflowRequest.For<TestWorkflow>(null));
        }

        [Test]
        public void Populates_properties_from_workflow_description()
        {
            var request = StartWorkflowRequest.For<TestWorkflow>("workflowId");

            Assert.That(request.WorkflowName, Is.EqualTo("TestWorkflow"));
            Assert.That(request.Version, Is.EqualTo("1.0"));
            Assert.That(request.WorkflowId, Is.EqualTo("workflowId"));
        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow : Workflow
        {
            
        }
    }
}