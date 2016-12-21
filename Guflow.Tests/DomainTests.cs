using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;
using ChildPolicy = Guflow.Decider.ChildPolicy;

namespace Guflow.Tests
{
    [TestFixture]
    public class DomainTests
    {
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflowClient;
        private Domain _domain;
        private const string _domainName = "name1";
        private WorkflowDescriptionAttribute _descriptionForTestWorkflow;
        private TaskQueue _taskQueue;
        [SetUp]
        public void Setup()
        {
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain(_domainName, _amazonWorkflowClient.Object);
            _taskQueue = new TaskQueue("queuename");
            _descriptionForTestWorkflow = new WorkflowDescriptionAttribute("1.0")
            {
                Name = "TestWorkflow",
                DefaultChildPolicy = "ChildPolicy",
                DefaultLambdaRole = "lambda",
                DefaultTaskListName = "tname",
                DefaultTaskPriority = 10,
                Description = "desc",
                DefaultTaskStartToCloseTimeoutInSeconds = 11,
                DefaultExecutionStartToCloseTimeoutInSeconds = 12
            };
        }

        [Test]
        public async Task Register_the_workflow_when_it_is_not_already_registered()
        {
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", _domainName);

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            AssertThatAmazonSwfIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Can_register_the_workflow_using_workflow_description()
        {
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", _domainName);

            await _domain.RegisterWorkflowAsync(_descriptionForTestWorkflow);

            AssertThatAmazonSwfIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Register_the_workflow_when_version_is_different()
        {
            SetupAmazonSwfToReturn(new WorkflowTypeInfo() { WorkflowType = new WorkflowType() { Version = "2.0" }, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            AssertThatAmazonSwfIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Does_not_register_workflow_when_a_workflow_with_same_name_and_version_is_already_registered()
        {
            SetupAmazonSwfToReturn(new WorkflowTypeInfo { WorkflowType = new WorkflowType { Version = "1.0" }, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            _amazonWorkflowClient.Verify(w => w.RegisterWorkflowTypeAsync(It.IsAny<RegisterWorkflowTypeRequest>(), default(CancellationToken)), Times.Never);
        }

        [Test]
        public async Task Register_the_domain_when_it_is_not_registered()
        {
            AmazonWorkflowReturnsEmptyDomainInfo();

            await _domain.RegisterAsync(10,"desc");

            AssertThatAmazonSwfIsSendDomainRegistrationRequest(retentionPeriod:10, desc:"desc");
        }

        [Test]
        public async Task Does_not_register_domain_when_it_is_already_registered()
        {
            SetupAmazonSwfToReturn(new DomainInfo { Name = _domainName, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterAsync(10, "desc");

            _amazonWorkflowClient.Verify(c=>c.RegisterDomainAsync(It.IsAny<RegisterDomainRequest>(),It.IsAny<CancellationToken>()),Times.Never);
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentException>(() => new Domain(null, _amazonWorkflowClient.Object));
            Assert.Throws<ArgumentNullException>(() => new Domain("name", (IAmazonSimpleWorkflow)null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _domain.RegisterWorkflowAsync((WorkflowDescriptionAttribute) null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _domain.SignalWorkflowAsync(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _domain.CancelWorkflowAsync(null));
        }

        [Test]
        public async Task By_default_read_all_events_when_decision_task_is_returned_in_multiple_pages()
        {
            var decision1 = new DecisionTask {NextPageToken = "token", Events = new List<HistoryEvent> {new HistoryEvent {EventId = 1}}};
            var decision2 = new DecisionTask {NextPageToken = "token1", Events = new List<HistoryEvent> { new HistoryEvent { EventId =  2} } };
            var decision3 = new DecisionTask { Events = new List<HistoryEvent> { new HistoryEvent { EventId = 3 } } };
            AmazonSwfReturnsDecisionTask(decision1, decision2, decision3);
            var expectedEvents = decision1.Events.Concat(decision2.Events).Concat(decision3.Events).ToArray();

            var decisionTask = await _domain.PollForDecisionTaskAsync(_taskQueue);

            Assert.That(decisionTask.Events, Is.EquivalentTo(expectedEvents));
        }

        [Test]
        public async Task Task_queue_can_be_configured_to_read_first_page_of_hisotry_events()
        {
            var decision1 = new DecisionTask { NextPageToken = "token", Events = new List<HistoryEvent> { new HistoryEvent { EventId = 1 } } };
            var decision2 = new DecisionTask { NextPageToken = "token1", Events = new List<HistoryEvent> { new HistoryEvent { EventId = 2 } } };
            var decision3 = new DecisionTask { Events = new List<HistoryEvent> { new HistoryEvent { EventId = 3 } } };
            AmazonSwfReturnsDecisionTask(decision1, decision2, decision3);

            _taskQueue.ReadStrategy = TaskQueue.ReadFirstPage;

            var decisionTask = await _domain.PollForDecisionTaskAsync(_taskQueue);

            Assert.That(decisionTask.Events, Is.EquivalentTo(decision1.Events));
        }

   
        [Test]
        public async Task Send_signal_request_to_amazon_swf()
        {
            var signalRequest = new SignalWorkflowRequest("workflowId", "signalName")
            {
                WorkflowRunId = "runid",
                SignalInput = "input"
            };

            await _domain.SignalWorkflowAsync(signalRequest);

            AssertThatAmazonIsSend(signalRequest);
        }

        [Test]
        public async Task Send_cancel_request_to_amazon_swf()
        {
            var cancelRequest = new CancelWorkflowRequest("workflowId") {WorkflowRunId = "runid"};

            await _domain.CancelWorkflowAsync(cancelRequest);

            AssertThatAmazonSwfIsSend(cancelRequest);
        }

        [Test]
        public async Task Send_workflow_start_request_to_amazon_swf()
        {
            var startRequest = new StartWorkflowRequest("workflowName", "version", "workflowId")
            {
                ChildPolicy = ChildPolicy.Abandon,
                TaskListName = "tlist",
                ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(10),
                Input = "input",
                LambdaRole = "lrole",
                Tags = new List<string> { "tag1", "tag2"},
                TaskPriority = 2,
                TaskStartToCloseTimeout = TimeSpan.FromSeconds(23)
            };

            await _domain.StartWorkflowAsync(startRequest);

            AssertThatAmazonSwfIsSend(startRequest);
        }

        private void AmazonSwfReturnsDecisionTask(DecisionTask decisionTask1, DecisionTask decisionTask2, DecisionTask decisionTask3)
        {
            _amazonWorkflowClient.SetupSequence(c => c.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse { DecisionTask = decisionTask1 }))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse { DecisionTask = decisionTask2 }))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse { DecisionTask = decisionTask3 }));
        }


        private void AssertThatAmazonSwfIsSend(StartWorkflowRequest request)
        {
            Func<StartWorkflowExecutionRequest, bool> startRequest = s =>
            {
                Assert.That(request.WorkflowId, Is.EqualTo(s.WorkflowId));
                Assert.That(request.WorkflowName, Is.EqualTo(s.WorkflowType.Name));
                Assert.That(request.Version, Is.EqualTo(s.WorkflowType.Version));
                Assert.That(request.Input, Is.EqualTo(s.Input));
                Assert.That(request.ChildPolicy, Is.EqualTo(s.ChildPolicy.Value));
                Assert.That(request.LambdaRole, Is.EqualTo(s.LambdaRole));
                Assert.That(request.TaskListName, Is.EqualTo(s.TaskList.Name));
                Assert.That(request.TaskPriority, Is.EqualTo(int.Parse(s.TaskPriority)));
                Assert.That(request.Tags, Is.EqualTo(s.TagList));
                Assert.That(request.TaskStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(int.Parse(s.TaskStartToCloseTimeout))));
                Assert.That(request.ExecutionStartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(int.Parse(s.ExecutionStartToCloseTimeout))));
                Assert.That(_domainName, Is.EqualTo(s.Domain));
                return true;
            };
            _amazonWorkflowClient.Verify(c => c.StartWorkflowExecutionAsync(
                                                                            It.Is<StartWorkflowExecutionRequest>((start) => startRequest(start)),
                                                                            It.IsAny<CancellationToken>()),
                                                                            Times.Once);
        }

        private void AssertThatAmazonSwfIsSend(CancelWorkflowRequest request)
        {
            Func<RequestCancelWorkflowExecutionRequest, bool> cancelRequest = s =>
            {
                Assert.That(request.WorkflowId, Is.EqualTo(s.WorkflowId));
                Assert.That(request.WorkflowRunId, Is.EqualTo(request.WorkflowRunId));
                Assert.That(_domainName, Is.EqualTo(s.Domain));
                return true;
            };
            _amazonWorkflowClient.Verify(c => c.RequestCancelWorkflowExecutionAsync(
                                                                            It.Is<RequestCancelWorkflowExecutionRequest>((signal) => cancelRequest(signal)),
                                                                            It.IsAny<CancellationToken>()), 
                                                                            Times.Once);
        }

        private void AssertThatAmazonIsSend(SignalWorkflowRequest request)
        {
            Func<SignalWorkflowExecutionRequest, bool> signalRequest = s =>
            {
                Assert.That(request.WorkflowId, Is.EqualTo(s.WorkflowId));
                Assert.That(request.WorkflowRunId, Is.EqualTo(request.WorkflowRunId));
                Assert.That(request.Signalname, Is.EqualTo(s.SignalName));
                Assert.That(request.SignalInput, Is.EqualTo(s.Input));
                Assert.That(_domainName, Is.EqualTo(s.Domain));
                return true;
            };
            _amazonWorkflowClient.Verify(c => c.SignalWorkflowExecutionAsync(
                                            It.Is<SignalWorkflowExecutionRequest>((signal) => signalRequest(signal)), 
                                            It.IsAny<CancellationToken>()), 
                                            Times.Once);
        }

        private void AssertThatAmazonSwfIsSendDomainRegistrationRequest(int retentionPeriod, string desc)
        {
            Func<RegisterDomainRequest, bool> request = r =>
            {
                Assert.That(r.Name, Is.EqualTo(_domainName));
                Assert.That(r.Description, Is.EqualTo(desc));
                Assert.That(r.WorkflowExecutionRetentionPeriodInDays, Is.EqualTo(retentionPeriod.ToString()));
                return true;
            };
            _amazonWorkflowClient.Verify(c=>c.RegisterDomainAsync(It.Is<RegisterDomainRequest>(r=>request(r)),It.IsAny<CancellationToken>()),Times.Once);
        }

        private void SetupAmazonSwfToReturn(params DomainInfo[] domainInfo)
        {
            var listDomainsResponse = new ListDomainsResponse();
            listDomainsResponse.DomainInfos = new DomainInfos()
            {
                Infos = domainInfo.ToList()
            };
            _amazonWorkflowClient.Setup(a => a.ListDomainsAsync(It.IsAny<ListDomainsRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(listDomainsResponse));
        }

        private void AmazonWorkflowReturnsEmptyDomainInfo()
        {
            Action<ListDomainsRequest, CancellationToken> requestParameters = (r, c) =>
            {
                Assert.That(r.MaximumPageSize,Is.EqualTo(1000));
                Assert.That(r.NextPageToken,Is.Null);
                Assert.That(r.ReverseOrder,Is.False);
            };

            var emptyDomainResponse = new ListDomainsResponse();
            emptyDomainResponse.DomainInfos = new DomainInfos()
            {
                Infos = new List<DomainInfo>()
            };

            _amazonWorkflowClient.Setup(a => a.ListDomainsAsync(It.IsAny<ListDomainsRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(emptyDomainResponse)).Callback(requestParameters);
        }

        private void SetupAmazonSwfToReturn(params WorkflowTypeInfo[] workflowTypeInfos)
        {
            var listWorkflowTypeResponse = new ListWorkflowTypesResponse();
            listWorkflowTypeResponse.WorkflowTypeInfos = new WorkflowTypeInfos()
            {
                TypeInfos = workflowTypeInfos.ToList()
            };
            _amazonWorkflowClient.Setup(a => a.ListWorkflowTypesAsync(It.IsAny<ListWorkflowTypesRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(listWorkflowTypeResponse));
        }

        private void AmazonWorkflowReturnsEmptyListFor(string workflowName, string domainName)
        {
            Action<ListWorkflowTypesRequest, CancellationToken> requestedParameters = (r, c) =>
            {
                Assert.That(r.Name, Is.EqualTo(workflowName));
                Assert.That(r.Domain, Is.EqualTo(domainName));
                Assert.That(r.MaximumPageSize, Is.EqualTo(1000));
                Assert.That(c, Is.EqualTo(default(CancellationToken)));
                Assert.That(r.RegistrationStatus, Is.EqualTo(RegistrationStatus.REGISTERED));
            };
            var emptyListResponse = new ListWorkflowTypesResponse() { WorkflowTypeInfos = new WorkflowTypeInfos() { TypeInfos = new List<WorkflowTypeInfo>() } };
            _amazonWorkflowClient.Setup(a => a.ListWorkflowTypesAsync(It.IsAny<ListWorkflowTypesRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(emptyListResponse)).Callback(requestedParameters);
        }
        private void AssertThatAmazonSwfIsSendRegistrationRequest(WorkflowDescriptionAttribute attribute)
        {
            Func<RegisterWorkflowTypeRequest, bool> parameter = (r) =>
            {
                Assert.That(r.Name, Is.EqualTo(attribute.Name));
                Assert.That(r.Version, Is.EqualTo(attribute.Version));
                Assert.That(r.Description, Is.EqualTo(attribute.Description));
                Assert.That(r.Domain, Is.EqualTo(_domainName));
                Assert.That(r.DefaultTaskList.Name, Is.EqualTo(attribute.DefaultTaskListName));
                Assert.That(r.DefaultChildPolicy.Value, Is.EqualTo(attribute.DefaultChildPolicy));
                Assert.That(r.DefaultLambdaRole, Is.EqualTo(attribute.DefaultLambdaRole));
                Assert.That(r.DefaultExecutionStartToCloseTimeout, Is.EqualTo(attribute.DefaultExecutionStartToCloseTimeout));
                Assert.That(r.DefaultTaskStartToCloseTimeout, Is.EqualTo(attribute.DefaultTaskStartToCloseTimeout));
                return true;
            };

            _amazonWorkflowClient.Verify(a => a.RegisterWorkflowTypeAsync(It.Is<RegisterWorkflowTypeRequest>(req => parameter(req)), default(CancellationToken)), Times.Once);
        }


        [WorkflowDescription("1.0", DefaultChildPolicy = "ChildPolicy", DefaultLambdaRole = "lambda", DefaultTaskListName = "tname", DefaultTaskPriority = 10,
            Description = "desc", DefaultTaskStartToCloseTimeoutInSeconds = 11, DefaultExecutionStartToCloseTimeoutInSeconds = 12)]
        private class TestWorkflow : Workflow
        {
        }
    }
}