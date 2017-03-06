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

        [SetUp]
        public void Setup()
        {
            var domain = new Domain("name", new Mock<IAmazonSimpleWorkflow>().Object);
            _hostedActivities = new HostedActivities(domain, new Type[] { typeof(TestActivity) });
        }
        [Test]
        public async Task On_execution_Empty_worker_task_return_deferred_activity_response()
        {
            var workerTask = WorkerTask.Empty;

            var response = await workerTask.ExecuteFor(_hostedActivities);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defferred));
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

            Assert.That(response, Is.EqualTo(new ActivityCompletedResponse("token" ,"result")));
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
    }
}