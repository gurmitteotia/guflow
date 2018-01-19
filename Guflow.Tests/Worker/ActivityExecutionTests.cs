// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Concurrent;
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
    public class ActivityExecutionTests
    {
        private ActivityHost _activityHost;
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;
        private const string ActivityResult = "result";
        private const string TaskToken = "token";
       
        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var domain = new Domain("name", _amazonSimpleWorkflow.Object);
            _activityHost = new ActivityHost(domain, new[] { typeof(TestActivity) }, t=> new TestActivity(ActivityResult));
            TestActivity.Reset();
        }
        [Test]
        public async Task Can_limit_the_activity_execution()
        {
            var concurrentExecution = ActivityExecution.Concurrent(3);
            concurrentExecution.Set(_activityHost);

            for(int i=0;i<20;i++)
                await concurrentExecution.ExecuteAsync(NewWorkerTask());

            Assert.That(TestActivity.MaxConcurrentExecution , Is.EqualTo(3));
        }

        [Test]
        public async Task Can_execute_tasks_in_sequence()
        {
            var concurrentExecution = ActivityExecution.Sequencial;
            concurrentExecution.Set(_activityHost);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());


            Assert.That(TestActivity.MaxConcurrentExecution, Is.EqualTo(1));
        }

        [Test]
        public async Task Execute_activity_in_sequence_when_concurrent_limit_is_one()
        {
            var concurrentExecution = ActivityExecution.Concurrent(1);
            concurrentExecution.Set(_activityHost);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());


            Assert.That(TestActivity.MaxConcurrentExecution, Is.EqualTo(1));
        }

        [Test]
        public void Throws_exception_when_limit_is_zero()
        {
            Assert.Throws<ArgumentException>(() => ActivityExecution.Concurrent(0));
        }

        [Test]
        public async Task Send_activity_response_to_amazon_client()
        {
            var concurrentExecution = ActivityExecution.Sequencial;
            concurrentExecution.Set(_activityHost);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());

            Func<RespondActivityTaskCompletedRequest, bool> request = r =>
            {
                Assert.That(r.Result, Is.EqualTo(ActivityResult));
                Assert.That(r.TaskToken, Is.EqualTo(TaskToken));
                return true;
            };
            _amazonSimpleWorkflow.Verify(w=>w.RespondActivityTaskCompletedAsync(It.Is<RespondActivityTaskCompletedRequest>(r=>request(r)), It.IsAny<CancellationToken>()), Times.Once);
        }

      
        private static WorkerTask NewWorkerTask()
        {
            return WorkerTask.CreateFor(new ActivityTask()
            {
                ActivityType = new ActivityType() { Name = "TestActivity", Version = "1.0" },
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution(){ RunId = "rid", WorkflowId = "wid"},
                Input = "input"
            }, Mock.Of<IHeartbeatSwfApi>());
        }
        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {
            private static int _noOfConcurrentTasks;
            private static ConcurrentBag<int> _concurrentTaskRecords = new ConcurrentBag<int>();
            private static readonly Random _random = new Random();
            private readonly string _result;

            public TestActivity(string result)
            {
                _result = result;
            }

            [ActivityMethod]
            public async Task<ActivityResponse> Execute()
            {
                var concurrentTasks = Interlocked.Increment(ref _noOfConcurrentTasks);
                Console.WriteLine($"Concurrent tasks{concurrentTasks}");
                _concurrentTaskRecords.Add(concurrentTasks);
                await Task.Delay(_random.Next(10,100));
                Interlocked.Decrement(ref _noOfConcurrentTasks);
                return Complete(_result);
            }

            public static int MaxConcurrentExecution => _concurrentTaskRecords.Max();

            public static void Reset()
            {
                _noOfConcurrentTasks = 0;
                _concurrentTaskRecords = new ConcurrentBag<int>();
            }
        }
    }
}