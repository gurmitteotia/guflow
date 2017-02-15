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
        public async Task Emptry_worker_task_return_deferred_activity_response_on_execution()
        {
            var workerTask = WorkerTask.Empty;

            var response = await workerTask.ExecuteFor(_hostedActivities);

            Assert.That(response, Is.EqualTo(ActivityResponse.Defferred));
        }

        [Test]
        public async Task Return_activity_response_for_activity_task()
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

        [ActivityDescription("1.0")]
        private class TestActivity : Activity
        {

            [Execute]
            public async Task<ActivityResponse> Execute()
            {
                await Task.Delay(10);
                return Completed("result");
            }
        }
    }
}