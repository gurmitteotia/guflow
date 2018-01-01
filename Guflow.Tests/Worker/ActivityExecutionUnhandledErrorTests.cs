using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ActivityExecutionUnhandledErrorTests
    {
        private ActivitiesHost _activitiesHost;
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;
        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var domain = new Domain("name", _amazonSimpleWorkflow.Object);
            _activitiesHost = new ActivitiesHost(domain, new[] { typeof(TestActivity) });
        }
        [Test]
        public async Task Fault_the_activity_host_when_execution_excepiton_is_unhandled()
        {
            var concurrentExecution = ActivityExecution.Sequencial;
            concurrentExecution.Set(_activitiesHost);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());

            Assert.That(_activitiesHost.Status , Is.EqualTo(HostStatus.Faulted));
        }
        private static WorkerTask NewWorkerTask()
        {
            return WorkerTask.CreateFor(new ActivityTask()
            {
                ActivityType = new ActivityType() { Name = "TestActivity", Version = "1.0" },
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution() { RunId = "rid", WorkflowId = "wid" },
                Input = "input"
            });
        }

        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {
            public TestActivity()
            {
                FailOnException = false;
            }
            [ActivityMethod]
            public void Execute()
            {
                throw new Exception("message");
            }
        }
    }
}