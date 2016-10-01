using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string _domainName = "name1";
        private WorkflowDescriptionAttribute _descriptionForTestWorkflow;
        [SetUp]
        public void Setup()
        {
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _workflowClient = new WorkflowClient(_amazonWorkflowClient.Object);
            _descriptionForTestWorkflow = new WorkflowDescriptionAttribute("1.0")
            {
                Name = "TestWorkflow", DefaultChildPolicy = "ChildPolicy", DefaultLambdaRole = "lambda", DefaultTaskListName = "tname",
                DefaultTaskPriority = 10, Description = "desc", Domain = _domainName, DefaultTaskStartToCloseTimeoutInSeconds = 11,
                DefaultExecutionStartToCloseTimeoutInSeconds = 12
            };
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
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow",_domainName);
            
            await _workflowClient.Register<TestWorkflow>();

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Can_register_the_workflow_using_workflow_description()
        {
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", _domainName);

            await _workflowClient.Register(_descriptionForTestWorkflow);

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Register_the_workflow_when_version_is_different()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo() { WorkflowType = new WorkflowType() { Version = "2.0" }, Status = RegistrationStatus.REGISTERED });

            await _workflowClient.Register<TestWorkflow>();

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Does_not_register_workflow_when_a_workflow_with_same_name_and_version_is_already_registered()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo() { WorkflowType = new WorkflowType() { Version = "1.0" }, Status = RegistrationStatus.REGISTERED });

            await _workflowClient.Register<TestWorkflow>();

            _amazonWorkflowClient.Verify(w=>w.RegisterWorkflowTypeAsync(It.IsAny<RegisterWorkflowTypeRequest>(),default(CancellationToken)),Times.Never);
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

        private void AmazonWorkflowReturnsEmptyListFor(string workflowName, string domainName)
        {
            Action<ListWorkflowTypesRequest,CancellationToken> requestedParameters = (r,c) =>
            {
                Assert.That(r.Name,Is.EqualTo(workflowName));
                Assert.That(r.Domain,Is.EqualTo(domainName));
                Assert.That(r.MaximumPageSize,Is.EqualTo(1000));
                Assert.That(c,Is.EqualTo(default(CancellationToken)));
            };
            var emptyListResponse = new ListWorkflowTypesResponse(){WorkflowTypeInfos = new WorkflowTypeInfos(){ TypeInfos = new List<WorkflowTypeInfo>()}};
            _amazonWorkflowClient.Setup(a => a.ListWorkflowTypesAsync(It.IsAny<ListWorkflowTypesRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(emptyListResponse)).Callback(requestedParameters);
        }


        private void AssertThatAmazonIsSendRegistrationRequest(WorkflowDescription attribute)
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
                Assert.That(r.DefaultExecutionStartToCloseTimeout,Is.EqualTo(attribute.DefaultExecutionStartToCloseTimeout));
                Assert.That(r.DefaultTaskStartToCloseTimeout,Is.EqualTo(attribute.DefaultTaskStartToCloseTimeout));
                return true;
            };

            _amazonWorkflowClient.Verify(a=>a.RegisterWorkflowTypeAsync(It.Is<RegisterWorkflowTypeRequest>(req=>parameter(req)),default(CancellationToken)),Times.Once);
        }


        [WorkflowDescription("1.0",DefaultChildPolicy = "ChildPolicy",DefaultLambdaRole = "lambda",DefaultTaskListName = "tname",DefaultTaskPriority = 10,
            Description = "desc",Domain = _domainName, DefaultTaskStartToCloseTimeoutInSeconds = 11, DefaultExecutionStartToCloseTimeoutInSeconds = 12)]
        private class TestWorkflow : Workflow
        {
        }
    }
}