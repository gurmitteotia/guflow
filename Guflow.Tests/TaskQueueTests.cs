using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
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
        private TaskQueue _taskQueue;
        private Domain _domain;

        [SetUp]
        public void Setup()
        {
            _taskQueue = new TaskQueue(_taskListName, _pollingIdentity);
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain(_domainName,_amazonWorkflowClient.Object);
        }

        [Test]
        public async Task Returns_empty_workflow_task_when_no_new_task_are_returned_from_amazon_swf()
        {
            AmazonSwfReturnsDecisionTask(new DecisionTask());

            var workflowTasks = await _taskQueue.PollForNewTasksAsync(_domain);

            Assert.That(workflowTasks, Is.EqualTo(WorkflowTask.Empty));
        }

        [Test]
        public async Task Returns_non_empty_workflow_task_when_new_tasks_are_returned_from_amazon_swf()
        {
            AmazonSwfReturnsDecisionTask(new DecisionTask() { TaskToken = "token" });

            var workflowTasks = await _taskQueue.PollForNewTasksAsync(_domain);

            Assert.That(workflowTasks, Is.Not.EqualTo(WorkflowTask.Empty));
        }

        [Test]
        public async Task Polling_for_new_tasks_makes_request_to_amazon_swf()
        {
            SetupAmazonSwfClientToMakeRequest();

            await _taskQueue.PollForNewTasksAsync(_domain);

            _amazonWorkflowClient.Verify();
        }

        [Test]
        public async Task By_default_make_polling_request_with_computer_name_as_identity()
        {
            SetupAmazonSwfClientToMakeRequestWithPollingIdentity(Environment.MachineName);
            var taskQueue = new TaskQueue(_taskListName);

            await taskQueue.PollForNewTasksAsync(_domain);

            _amazonWorkflowClient.Verify();
        }

        [Test]
        public void Invalid_arugments_tests()
        {
            Assert.Throws<ArgumentException>(() => new TaskQueue(null));
            Assert.Throws<ArgumentException>(() => _taskQueue.ReadStrategy=null);
            Assert.Throws<ArgumentException>(() => _taskQueue.OnPollingError(null));
        }

        private void AmazonSwfReturnsDecisionTask(DecisionTask decisionTask)
        {
            _amazonWorkflowClient.Setup(c => c.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse() { DecisionTask = decisionTask }));
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