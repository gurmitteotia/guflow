using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;
namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowClientTests
    {
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflowClient;
        private WorkflowClient _workflowClient;

        [SetUp]
        public void Setup()
        {
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _workflowClient = new WorkflowClient(_amazonWorkflowClient.Object);
        }

        [Test]
        public void Register_throws_exception_when_workflow_is_deprecated()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo(){ WorkflowType = new WorkflowType(){Version = "1.0"}, Status = RegistrationStatus.DEPRECATED});
            
            Assert.Throws<WorkflowDeprecatedException>(async ()=> await _workflowClient.Register<TestWorkflow>());
        }

   
        [Test]
        public async Task Register_the_workflow_when_it_is_not_already_registered()
        {
            AmazonWorkflowReturns();
            
            await _workflowClient.Register<TestWorkflow>();

            AssertThatAmazonIsSendRegistrationRequest(WorkflowDescriptionAttribute.FindOn(typeof(TestWorkflow)));
        }

        [Test]
        public void Register_the_workflow_when_version_is_different()
        {

        }

        private void AmazonWorkflowReturns(params WorkflowTypeInfo [] workflowTypeInfos)
        {
            var listWorkflowTypeResponse = new ListWorkflowTypesResponse();
            listWorkflowTypeResponse.WorkflowTypeInfos = new WorkflowTypeInfos()
            {
                TypeInfos = workflowTypeInfos.ToList()
            };
            _amazonWorkflowClient.Setup(a => a.ListWorkflowTypesAsync(It.IsAny<ListWorkflowTypesRequest>(),default(CancellationToken)))
                .Returns(Task.FromResult(listWorkflowTypeResponse));
        }


        private void AssertThatAmazonIsSendRegistrationRequest(WorkflowDescriptionAttribute attribute)
        {
            Func<RegisterWorkflowTypeRequest,bool> parameter = (r) =>
            {
                Assert.That(r.Name,Is.EqualTo(attribute.Name));
                Assert.That(r.Version,Is.EqualTo(attribute.Version));
                Assert.That(r.Description,Is.EqualTo(attribute.Description));
                Assert.That(r.Domain,Is.EqualTo(attribute.Domain));
                Assert.That(r.DefaultTaskList.Name,Is.EqualTo(attribute.DefaultTaskListName));
                Assert.That(r.DefaultChildPolicy.Value,Is.EqualTo(attribute.DefaultChildPolicy));
                Assert.That(r.DefaultLambdaRole,Is.EqualTo(attribute.DefaultLambdaRole));
                Assert.That(r.DefaultExecutionStartToCloseTimeout,Is.EqualTo(attribute.ExecutionStartToCloseTimeoutInSeconds));
                Assert.That(r.DefaultTaskStartToCloseTimeout,Is.EqualTo(attribute.DefaultTaskStartToCloseTimeoutInSeconds));
                return true;
            };

            _amazonWorkflowClient.Verify(a=>a.RegisterWorkflowTypeAsync(It.Is<RegisterWorkflowTypeRequest>(req=>parameter(req)),default(CancellationToken)),Times.Once);
        }


        [WorkflowDescription("1.0")]
        private class TestWorkflow : Workflow
        {
        }
    }
}