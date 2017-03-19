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
    public class WorkerTaskTests
    {
        private HostedActivities _hostedActivities;
        private Domain _domain;

        [SetUp]
        public void Setup()
        {
            _domain = new Domain("name", new Mock<IAmazonSimpleWorkflow>().Object);
            _hostedActivities = new HostedActivities(_domain, new Type[] { typeof(TestActivity) });
        }
        [Test]
        public async Task On_execution_Empty_worker_task_return_deferred_activity_response()
        {
            var workerTask = WorkerTask.Empty;

            var response = await workerTask.ExecuteFor(_hostedActivities);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defer));
        }

        [Test]
        public async Task On_execution_returns_activity_response_for_activity_task()
        {
            var workerTask = WorkerTask.CreateFor(new ActivityTask
                                                {
                                                    ActivityType = new ActivityType() { Name = "TestActivity", Version = "1.0" },
                                                    WorkflowExecution = new WorkflowExecution(){ RunId = "runid", WorkflowId = "wid"},
                                                    TaskToken = "token"
                                                });

            var response = await workerTask.ExecuteFor(_hostedActivities);

            Assert.That(response, Is.EqualTo(new ActivityCompleteResponse("token" ,"result")));
        }

        [Test]
        public async Task Pass_activity_task_prorperties_to_activity_args()
        {
            var activityTask = new ActivityTask
            {
                ActivityType = new ActivityType {Name = "TestActivity", Version = "1.0"},
                WorkflowExecution = new WorkflowExecution {RunId = "runid", WorkflowId = "wid"},
                TaskToken = "token",
                Input = "input",
                ActivityId = "activityId",
                StartedEventId = 10
            };
            var workerTask = WorkerTask.CreateFor(activityTask);

            await workerTask.ExecuteFor(_hostedActivities);

           Assert.That(TestActivity.ActivityArgs.Input, Is.EqualTo(activityTask.Input));
           Assert.That(TestActivity.ActivityArgs.ActivityId, Is.EqualTo(activityTask.ActivityId));
           Assert.That(TestActivity.ActivityArgs.WorkflowId, Is.EqualTo(activityTask.WorkflowExecution.WorkflowId));
           Assert.That(TestActivity.ActivityArgs.WorkflowRunId, Is.EqualTo(activityTask.WorkflowExecution.RunId));
           Assert.That(TestActivity.ActivityArgs.TaskToken, Is.EqualTo(activityTask.TaskToken));
           Assert.That(TestActivity.ActivityArgs.StartedEventId, Is.EqualTo(activityTask.StartedEventId));
        }

        [Test]
        public async Task Execution_exception_can_be_handled_to_retry()
        {
            var workerTask = WorkerTask.CreateFor(new ActivityTask
            {
                ActivityType = new ActivityType() { Name = "ActivityThrowsException", Version = "1.0" },
                WorkflowExecution = new WorkflowExecution() { RunId = "runid", WorkflowId = "wid" },
                TaskToken = "token"
            });
            var hostedActivities = new HostedActivities(_domain, new [] { typeof(ActivityThrowsException) });
            workerTask.SetErrorHandler(ErrorHandler.Default(e => ErrorAction.Retry));
            
            var response = await workerTask.ExecuteFor(hostedActivities);

            Assert.That(response, Is.EqualTo(new ActivityCompleteResponse("token", "result")));

        }

        [Test]
        public void By_default_execution_exception_are_not_handled()
        {
            var workerTask = WorkerTask.CreateFor(new ActivityTask
            {
                ActivityType = new ActivityType() { Name = "ActivityThrowsException", Version = "1.0" },
                WorkflowExecution = new WorkflowExecution() { RunId = "runid", WorkflowId = "wid" },
                TaskToken = "token"
            });
            var hostedActivities = new HostedActivities(_domain, new[] { typeof(ActivityThrowsException) });

            Assert.ThrowsAsync<InvalidOperationException>(async ()=>await workerTask.ExecuteFor(hostedActivities));
        }

        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {

            [Execute]
            public async Task<ActivityResponse> Execute(ActivityArgs args)
            {
                await Task.Delay(10);
                ActivityArgs = args;
                return Complete("result");
            }

            public static ActivityArgs ActivityArgs;
        }

        [ActivityDescription("1.0")]
        private class ActivityThrowsException : Activity
        {
            private int _retryCount = 0;

            public ActivityThrowsException()
            {
                FailOnException = false;
            }
            [Execute]
            public ActivityResponse Execute(ActivityArgs args)
            {
                _retryCount++;
                if(_retryCount == 1)
                    throw new InvalidOperationException("exception");
                return Complete("result");
            }
        }
    }
}