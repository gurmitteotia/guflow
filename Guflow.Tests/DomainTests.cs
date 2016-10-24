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
        public void Register_throws_exception_when_workflow_is_deprecated()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo { WorkflowType = new WorkflowType() { Version = "1.0" }, Status = RegistrationStatus.DEPRECATED });

            Assert.Throws<WorkflowDeprecatedException>(async () => await _domain.RegisterWorkflowAsync<TestWorkflow>());
        }
        [Test]
        public async Task Register_the_workflow_when_it_is_not_already_registered()
        {
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", _domainName);

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Can_register_the_workflow_using_workflow_description()
        {
            AmazonWorkflowReturnsEmptyListFor("TestWorkflow", _domainName);

            await _domain.RegisterWorkflowAsync(_descriptionForTestWorkflow);

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Register_the_workflow_when_version_is_different()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo() { WorkflowType = new WorkflowType() { Version = "2.0" }, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            AssertThatAmazonIsSendRegistrationRequest(_descriptionForTestWorkflow);
        }
        [Test]
        public async Task Does_not_register_workflow_when_a_workflow_with_same_name_and_version_is_already_registered()
        {
            AmazonWorkflowReturns(new WorkflowTypeInfo() { WorkflowType = new WorkflowType() { Version = "1.0" }, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterWorkflowAsync<TestWorkflow>();

            _amazonWorkflowClient.Verify(w => w.RegisterWorkflowTypeAsync(It.IsAny<RegisterWorkflowTypeRequest>(), default(CancellationToken)), Times.Never);
        }

        [Test]
        public void Register_throws_exception_when_domain_is_deprecated()
        {
            AmazonWorkflowReturns(new DomainInfo(){ Name = _domainName, Status = RegistrationStatus.DEPRECATED});

            Assert.Throws<DomainDeprecatedException>(async () => await _domain.RegisterAsync(1));
        }

        [Test]
        public async Task Register_the_domain_when_it_is_not_registered()
        {
            AmazonWorkflowReturnsEmptyDomainInfo();

            await _domain.RegisterAsync(10,"desc");

            AssertThatAmazonIsSendDomainRegistrationRequest(retentionPeriod:10, desc:"desc");
        }

        [Test]
        public async Task Does_not_register_domain_when_it_is_already_registered()
        {
            AmazonWorkflowReturns(new DomainInfo() { Name = _domainName, Status = RegistrationStatus.REGISTERED });

            await _domain.RegisterAsync(10, "desc");

            _amazonWorkflowClient.Verify(c=>c.RegisterDomainAsync(It.IsAny<RegisterDomainRequest>(),It.IsAny<CancellationToken>()),Times.Never);
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            Assert.Throws<ArgumentException>(() => new Domain(null, _amazonWorkflowClient.Object));
            Assert.Throws<ArgumentException>(() => new Domain("name", null));
            Assert.Throws<ArgumentException>(async () => await _domain.RegisterWorkflowAsync((WorkflowDescriptionAttribute) null));
        }

        [Test]
        public async Task By_default_read_all_events_when_decision_task_is_returned_in_multiple_pages()
        {
            var decision1 = new DecisionTask() {NextPageToken = "token", Events = new List<HistoryEvent>(){new HistoryEvent(){EventId = 1}}};
            var decision2 = new DecisionTask() {NextPageToken = "token1", Events = new List<HistoryEvent>() { new HistoryEvent() { EventId =  2} } };
            var decision3 = new DecisionTask() { Events = new List<HistoryEvent>() { new HistoryEvent() { EventId = 3 } } };
            AmazonSwfReturnsDecisionTask(decision1, decision2, decision3);
            var expectedEvents = decision1.Events.Concat(decision2.Events).Concat(decision3.Events).ToArray();

            var decisionTask = await _domain.PollForDecisionTaskAsync(_taskQueue);

            Assert.That(decisionTask.Events, Is.EquivalentTo(expectedEvents));
        }

        [Test]
        public async Task Task_queue_can_be_configured_to_read_first_page_of_hisotry_events()
        {
            var decision1 = new DecisionTask() { NextPageToken = "token", Events = new List<HistoryEvent>() { new HistoryEvent() { EventId = 1 } } };
            var decision2 = new DecisionTask() { NextPageToken = "token1", Events = new List<HistoryEvent>() { new HistoryEvent() { EventId = 2 } } };
            var decision3 = new DecisionTask() { Events = new List<HistoryEvent>() { new HistoryEvent() { EventId = 3 } } };
            AmazonSwfReturnsDecisionTask(decision1, decision2, decision3);

            _taskQueue.ReadStrategy = TaskQueue.ReadFirstPage;

            var decisionTask = await _domain.PollForDecisionTaskAsync(_taskQueue);

            Assert.That(decisionTask.Events, Is.EquivalentTo(decision1.Events));
        }

        [Test]
        public async Task By_default_polling_exception_are_not_handled()
        {
            _amazonWorkflowClient.Setup(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                                 .Throws(new UnknownResourceException("not found"));

            Assert.Throws<UnknownResourceException>(async () => await _domain.PollForDecisionTaskAsync(_taskQueue));
        }

        [Test]
        public async Task Polling_exception_can_be_handled_to_retry()
        {
            var decisionTask = new DecisionTask();
            _amazonWorkflowClient.SetupSequence(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                                 .Throws(new UnknownResourceException("not found"))
                                 .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask }));

            var expectedDecisionTask = await _domain.PollForDecisionTaskAsync(_taskQueue);

            Assert.That(expectedDecisionTask, Is.EqualTo(decisionTask));
        }

        private void AmazonSwfReturnsDecisionTask(DecisionTask decisionTask1, DecisionTask decisionTask2, DecisionTask decisionTask3)
        {
            _amazonWorkflowClient.SetupSequence(c => c.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask1 }))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask2 }))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask3 }));
        }


        private void AssertThatAmazonIsSendDomainRegistrationRequest(int retentionPeriod, string desc)
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

        private void AmazonWorkflowReturns(params DomainInfo[] domainInfo)
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

        private void AmazonWorkflowReturns(params WorkflowTypeInfo[] workflowTypeInfos)
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
            };
            var emptyListResponse = new ListWorkflowTypesResponse() { WorkflowTypeInfos = new WorkflowTypeInfos() { TypeInfos = new List<WorkflowTypeInfo>() } };
            _amazonWorkflowClient.Setup(a => a.ListWorkflowTypesAsync(It.IsAny<ListWorkflowTypesRequest>(), default(CancellationToken)))
                .Returns(Task.FromResult(emptyListResponse)).Callback(requestedParameters);
        }
        private void AssertThatAmazonIsSendRegistrationRequest(WorkflowDescriptionAttribute attribute)
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