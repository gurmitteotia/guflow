﻿using System;
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
    public class ActivityExecutionUnhandledExceptionTests
    {
        private HostedActivities _hostedActivities;
        private Mock<IAmazonSimpleWorkflow> _amazonSimpleWorkflow;

        [SetUp]
        public void Setup()
        {
            _amazonSimpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var domain = new Domain("name", _amazonSimpleWorkflow.Object);
            _hostedActivities = new HostedActivities(domain, new[] { typeof(ErrorThrowingActivity) });
        }
       
        [Test]
        public async Task Fault_the_activity_host_when_execution_excepiton_is_unhandled()
        {
            var activityExecution = ActivityExecution.Sequencial;
            activityExecution.Set(_hostedActivities);

            await activityExecution.ExecuteAsync(NewWorkerTask());

            Assert.That(_hostedActivities.Status , Is.EqualTo(HostStatus.Faulted));
        }

        private static WorkerTask NewWorkerTask()
        {
            return WorkerTask.CreateFor(new ActivityTask()
            {
                ActivityType = new ActivityType() { Name = "ErrorThrowingActivity", Version = "1.0" },
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution() { RunId = "rid", WorkflowId = "wid" },
                Input = "input"
            });
        }

        [ActivityDescription("1.0")]
        private class ErrorThrowingActivity : Activity
        {
            public ErrorThrowingActivity()
            {
                FailOnException = false;
            }
            [Execute]
            public void Execute()
            {
                throw new Exception("message");
            }
        }
    }
}