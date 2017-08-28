using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TaskQueueTests
    {
        private const string _taskListName = "tname";
        private const string _domainName = "domain";
        private const string _pollingIdentity = "id";
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflowClient;
        private CancellationTokenSource _cancellationTokenSource;
        private TaskQueue _taskQueue;
        private Domain _domain;

        [SetUp]
        public void Setup()
        {
            _taskQueue = new TaskQueue(_taskListName, _pollingIdentity);
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _cancellationTokenSource = new CancellationTokenSource();
            _domain = new Domain(_domainName,_amazonWorkflowClient.Object);
        }

        [Test]
        public async Task Returns_empty_workflow_task_when_no_new_task_are_returned_from_amazon_swf()
        {
            AmazonSwfReturns(new DecisionTask());

            var workflowTasks = await _taskQueue.PollForWorkflowTaskAsync(_domain);

            Assert.That(workflowTasks, Is.EqualTo(WorkflowTask.Empty));
        }

        [Test]
        public async Task Returns_non_empty_workflow_task_when_new_tasks_are_returned_from_amazon_swf()
        {
            AmazonSwfReturns(new DecisionTask() { TaskToken = "token" });

            var workflowTasks = await _taskQueue.PollForWorkflowTaskAsync(_domain);

            Assert.That(workflowTasks, Is.Not.EqualTo(WorkflowTask.Empty));
        }

        [Test]
        public async Task Returns_empty_worker_task_when_no_new_task_are_returned_from_amazon_swf()
        {
            AmazonSwfReturns(new ActivityTask());

            var workerTask = await _taskQueue.PollForWorkerTaskAsync(_domain, _cancellationTokenSource.Token);

            Assert.That(workerTask, Is.EqualTo(WorkerTask.Empty));
        }
        [Test]
        public async Task Returns_empty_worker_task_when_null_response_is_returned_from_amazon_swf()
        {
            AmazonSwfReturns((ActivityTask)null);

            var workerTask = await _taskQueue.PollForWorkerTaskAsync(_domain, _cancellationTokenSource.Token);

            Assert.That(workerTask, Is.EqualTo(WorkerTask.Empty));
        }

        [Test]
        public async Task Returns_non_empty_worker_task_when_new_task_is_returned_from_amazon_swf()
        {
            AmazonSwfReturns(new ActivityTask { TaskToken = "token"});

            var workerTask = await _taskQueue.PollForWorkerTaskAsync(_domain, _cancellationTokenSource.Token);

            Assert.That(workerTask, Is.Not.EqualTo(WorkerTask.Empty));
        }

        [Test]
        public async Task Polling_for_new_workflow_tasks_makes_request_to_amazon_swf_with_populated_attributes()
        {
            SetupAmazonSwfClientToMakeRequest();

            await _taskQueue.PollForWorkflowTaskAsync(_domain);

            _amazonWorkflowClient.Verify();
        }

        [Test]
        public async Task By_default_make_polling_request_with_computer_name_as_identity()
        {
            SetupAmazonSwfClientToMakeRequestWithPollingIdentity(Environment.MachineName);
            var taskQueue = new TaskQueue(_taskListName);

            await taskQueue.PollForWorkflowTaskAsync(_domain);

            _amazonWorkflowClient.Verify();
        }

        [Test]
        public void Invalid_arugments_tests()
        {
            Assert.Throws<ArgumentException>(() => new TaskQueue(null));
            Assert.Throws<ArgumentNullException>(() => _taskQueue.ReadStrategy = null);
        }

        [Test]
        public void By_default_workflow_task_polling_exception_are_not_handled()
        {
            _amazonWorkflowClient.Setup(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                                 .Throws(new UnknownResourceException("not found"));

            Assert.ThrowsAsync<UnknownResourceException>(async () => await _taskQueue.PollForWorkflowTaskAsync(_domain));
        }


        private void AmazonSwfReturns(DecisionTask decisionTask)
        {
            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask }));
        }
        private void AmazonSwfReturns(ActivityTask activityTask)
        {
            _amazonWorkflowClient.Setup(c => c.PollForActivityTaskAsync(It.IsAny<PollForActivityTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForActivityTaskResponse() { ActivityTask = activityTask }));
        }
        private void SetupAmazonSwfClientToMakeRequest()
        {
            Func<PollForDecisionTaskRequest, bool> request = (r) =>
            {
                Assert.That(r.Identity, Is.EqualTo(_pollingIdentity));
                Assert.That(r.Domain, Is.EqualTo(_domainName));
                Assert.That(r.TaskList.Name, Is.EqualTo(_taskListName));
                Assert.That(r.MaximumPageSize, Is.EqualTo(1000));
                Assert.That(r.ReverseOrder, Is.True);
                Assert.That(r.NextPageToken, Is.Null.Or.Empty);
                return true;
            };

            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.Is<PollForDecisionTaskRequest>(r => request(r)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = new DecisionTask() })).Verifiable();
        }

        private void SetupAmazonSwfClientToMakeRequestWithPollingIdentity(string identity)
        {
            Func<PollForDecisionTaskRequest, bool> request = (r) =>
            {
                Assert.That(r.Identity, Is.EqualTo(identity));
                return true;
            };

            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.Is<PollForDecisionTaskRequest>(r => request(r)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = new DecisionTask() })).Verifiable();
        }
    }
}