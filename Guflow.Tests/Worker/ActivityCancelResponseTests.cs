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
    public class ActivityCancelResponseTests
    {

        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new ActivityCancelledResponse("details").Equals(new ActivityCancelledResponse("details")));
            Assert.IsTrue(new ActivityCancelledResponse(null).Equals(new ActivityCancelledResponse(null)));

            Assert.IsFalse(new ActivityCancelledResponse("details").Equals(new ActivityCancelledResponse("details1")));
            Assert.IsFalse(new ActivityCancelledResponse("details").Equals(new ActivityCancelledResponse(null)));
        }

        [Test]
        public async Task Send_cancel_response_to_amazon_swf()
        {
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityCancelledResponse("details");
            var cancellationTokenSource = new CancellationTokenSource();

            await response.SendAsync("token", simpleWorkflow.Object, cancellationTokenSource.Token);

            Func<RespondActivityTaskCanceledRequest, bool> request = r =>
            {
                Assert.That(r.TaskToken, Is.EqualTo("token"));
                Assert.That(r.Details, Is.EqualTo("details"));
                return true;
            };
            simpleWorkflow.Verify(s => s.RespondActivityTaskCanceledAsync(It.Is<RespondActivityTaskCanceledRequest>(r => request(r)), cancellationTokenSource.Token), Times.Once);
        }

        [Test]
        public void Details()
        {
            Assert.That(new ActivityCancelledResponse("details").Details, Is.EqualTo("details"));
        }
    }
}