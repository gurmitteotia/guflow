using System;
using System.Collections.Generic;
using System.Configuration;
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

        [Test]
        public void Throws_exception_when_domain_name_is_missing()
        {
            Assert.Throws<ConfigurationErrorsException>(async () => await _workflowClient.Register<WorkflowWithoutDomain>());
        }

        [Test]
        public async Task Override_domain_name_and_tasklist_name_with_workflow_clients_one_when_registering_the_workflow()
        {
            _workflowClient.Domain = "NewDomain";
            _workflowClient.TaskListName = "NewTaskListName";
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", "NewDomain");

            await _workflowClient.Register<TestWorkflow>();

            _descriptionForTestWorkflow.Domain = "NewDomain";
            _descriptionForTestWorkflow.DefaultTaskListName = "NewTaskListName";
            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }

        [Test]
        public async Task Returns_empty_workflow_task_when_no_new_task_are_returned_from_amazon_swf()
        {
            AmazonSwfReturnsDecisionTask(new DecisionTask());

            var workflowTasks = await _workflowClient.PollForNewTasks();

            Assert.That(workflowTasks,Is.EqualTo(WorkflowTasks.Empty));
        }

        [Test]
        public async Task Returns_non_empty_workflow_task_when_new_tasks_are_returned_from_amazon_swf()
        {
            AmazonSwfReturnsDecisionTask(new DecisionTask(){TaskToken = "token"});

            var workflowTasks = await _workflowClient.PollForNewTasks();

            Assert.That(workflowTasks, Is.Not.EqualTo(WorkflowTasks.Empty));
        }

        [Test]
        public async Task Polling_for_new_tasks_makes_request_to_amazon_swf()
        {
            SetupAmazonSwfClientToMakeRequest();

            await _workflowClient.PollForNewTasks();

            _amazonWorkflowClient.Verify();
            
        }

        [Test]
        public async Task Polling_use_retreival_strategy_when_next_page_token_is_non_empty()
        {
            var decisionTask = new DecisionTask() {TaskToken = "token", NextPageToken = "nextpageToken"};
            AmazonSwfReturnsDecisionTask(decisionTask);
            var retreivalStrategy = new Mock<IHistoryEventRetreivalStrategy>();
            retreivalStrategy.Setup(r => r.RetreiveEvents(decisionTask, _workflowClient)).Returns(new DecisionTask());
            _workflowClient.EventRetreivalStrategy = retreivalStrategy.Object;

            await _workflowClient.PollForNewTasks();

            
        }

        private void AmazonSwfReturnsDecisionTask(DecisionTask decisionTask)
        {
            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask}));
        }

        private void SetupAmazonSwfClientToMakeRequest()
        {
            Func<PollForDecisionTaskRequest, bool> request = (r) =>
            {
                Assert.That(r.Identity, Is.EqualTo(_workflowClient.Identity));
                Assert.That(r.Domain, Is.EqualTo(_workflowClient.Domain));
                Assert.That(r.TaskList.Name, Is.EqualTo(_workflowClient.TaskListName));
                Assert.That(r.MaximumPageSize, Is.EqualTo(1000));
                Assert.That(r.ReverseOrder, Is.True);
                Assert.That(r.NextPageToken, Is.Null.Or.Empty);
                return true;
            };

            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.Is<PollForDecisionTaskRequest>(r=>request(r)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = new DecisionTask() })).Verifiable();
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

        [WorkflowDescription("1.0")]
        private class WorkflowWithoutDomain : Workflow
        {
            
        }
    }
}