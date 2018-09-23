// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

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
    public class ActivityFailResponseTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new ActivityFailedResponse("reason" ,"details").Equals(new ActivityFailedResponse("reason", "details")));

            Assert.IsFalse(new ActivityFailedResponse("reason", "details").Equals(new ActivityFailedResponse("reason", "details1")));
            Assert.IsFalse(new ActivityFailedResponse("reason", "details").Equals(new ActivityFailedResponse("reason1", "details")));
            Assert.IsFalse(new ActivityFailedResponse("reason", "details").Equals(new ActivityFailedResponse( null, null)));
        }

        [Test]
        public async Task Send_activity_failed_response_to_amazon_swf()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityFailedResponse("reason", "details");

            await response.SendAsync("token", simpleWorkflow.Object, cancellationTokenSource.Token);

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