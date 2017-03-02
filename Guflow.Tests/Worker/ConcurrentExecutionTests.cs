using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
    [TestFixture]
    public class ConcurrentExecutionTests
    {
        private HostedActivities _hostedActivities;
        [SetUp]
        public void Setup()
        {
            _hostedActivities = new HostedActivities(new Domain("name", new Mock<IAmazonSimpleWorkflow>().Object), new[] { typeof(TestActivity) });
        }
        [Test]
        public async Task Can_limit_the_activity_execution()
        {
            var concurrentExecution = ConcurrentExecution.LimitTo(2);
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.Execute(CreateWorkerTask());
            await concurrentExecution.Execute(CreateWorkerTask());
            await concurrentExecution.Execute(CreateWorkerTask());
            await concurrentExecution.Execute(CreateWorkerTask());


            Assert.That(_concurrentTaskRecords.Max(), Is.EqualTo(2));
        }

        private WorkerTask CreateWorkerTask()
        {
            return WorkerTask.CreateFor(new ActivityTask()
            {
                ActivityType = new ActivityType() { Name = "TestActivity", Version = "1.0" },
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution(){ RunId = "rid", WorkflowId = "wid"},
                Input = "input"
            });
        }

        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {
            private static int _noOfConcurrentTasks;
            private static readonly ConcurrentBag<int> _concurrentTaskRecords = new ConcurrentBag<int>();

            [Execute]
            public async Task Execute()
            {
                _concurrentTaskRecords.Add(Interlocked.Increment(ref _noOfConcurrentTasks));
                await Task.Delay(100);
                Interlocked.Decrement(ref _noOfConcurrentTasks);
            }
        }
    }
}