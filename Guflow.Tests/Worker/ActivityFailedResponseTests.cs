﻿
using System;
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
    public class ActivityFailedResponseTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new ActivityFailedResponse("token", "reason" ,"details").Equals(new ActivityFailedResponse("token", "reason", "details")));

            Assert.IsFalse(new ActivityFailedResponse("token", "reason", "details").Equals(new ActivityFailedResponse("token", "reason", "details1")));
            Assert.IsFalse(new ActivityFailedResponse("token", "reason", "details").Equals(new ActivityFailedResponse("token", "reason1", "details")));
            Assert.IsFalse(new ActivityFailedResponse("token", "reason", "details").Equals(new ActivityFailedResponse("token1", "reason", "details")));
            Assert.IsFalse(new ActivityFailedResponse("token", "reason", "details").Equals(new ActivityFailedResponse("token1", null, null)));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new ActivityFailedResponse(null, "reason", "details"));
        }

        [Test]
        public async Task Send_activity_failed_response_to_amazon_swf()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityFailedResponse("token", "reason", "details");

            await response.SendAsync(simpleWorkflow.Object, cancellationTokenSource.Token);

            Func<RespondActivityTaskFailedRequest, bool> request = r =>
            {
                Assert.That(r.TaskToken, Is.EqualTo("token"));
                Assert.That(r.Reason, Is.EqualTo("reason"));
                Assert.That(r.Details, Is.EqualTo("details"));
                return true;
            };
            simpleWorkflow.Verify(s=> s.RespondActivityTaskFailedAsync(It.Is<RespondActivityTaskFailedRequest>(r=>request(r)), cancellationTokenSource.Token),Times.Once);
        }
        
    }
}