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
        private HostedActivities _hostedActivities;
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;
        private const string _activityResult = "result";
        private const string _taskToken = "token";
       
        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var domain = new Domain("name", _amazonSimpleWorkflow.Object);
            _hostedActivities = new HostedActivities(domain, new[] { typeof(TestActivity) }, t=> new TestActivity(_activityResult));
            TestActivity.Reset();
        }
        [Test]
        public async Task Can_limit_the_activity_execution()
        {
            var concurrentExecution = ActivityExecution.Concurrent(2);
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());
            await concurrentExecution.ExecuteAsync(NewWorkerTask());


            Assert.That(TestActivity.MaxConcurrentExecution , Is.EqualTo(2));
        }

        [Test]
        public async Task Can_execute_tasks_in_sequence()
        {
            var concurrentExecution = ActivityExecution.Sequencial;
            concurrentExecution.Set(_hostedActivities);

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
            concurrentExecution.Set(_hostedActivities);

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
            concurrentExecution.Set(_hostedActivities);

            await concurrentExecution.ExecuteAsync(NewWorkerTask());

            Func<RespondActivityTaskCompletedRequest, bool> request = r =>
            {
                Assert.That(r.Result, Is.EqualTo(_activityResult));
                Assert.That(r.TaskToken, Is.EqualTo(_taskToken));
                return true;
            };
            _amazonSimpleWorkflow.Verify(w=>w.RespondActivityTaskCompletedAsync(It.Is<RespondActivityTaskCompletedRequest>(r=>request(r)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Fault_the_activity_host_when_execution_excepiton_is_unhandled()
        {
            
        } 

        private static WorkerTask NewWorkerTask()
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
        private class ErrorThrowingActivity : Activity
        {
            [Execute]
            public void Execute()
            {
                throw new Exception("message");
            }
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

            [Execute]
            public async Task<ActivityResponse> Execute()
            {
                _concurrentTaskRecords.Add(Interlocked.Increment(ref _noOfConcurrentTasks));
                await Task.Delay(_random.Next(20,70));
                Interlocked.Decrement(ref _noOfConcurrentTasks);
                return Complete(_result);
            }

            public static int MaxConcurrentExecution
            {
                get { return _concurrentTaskRecords.Max(); }
            }

            public static void Reset()
            {
                _noOfConcurrentTasks = 0;
                _concurrentTaskRecords = new ConcurrentBag<int>();
            }
        }
    }
}