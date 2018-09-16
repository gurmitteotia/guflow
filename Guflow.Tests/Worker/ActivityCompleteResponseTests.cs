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
    public class ActivityCompleteResponseTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.IsTrue(new ActivityCompletedResponse("token", "result").Equals(new ActivityCompletedResponse("token", "result")));
            Assert.IsTrue(new ActivityCompletedResponse("token", null).Equals(new ActivityCompletedResponse("token", null)));


            Assert.IsFalse(new ActivityCompletedResponse("token", "result").Equals(new ActivityCompletedResponse("token", "result1")));
            Assert.IsFalse(new ActivityCompletedResponse("token", "result").Equals(new ActivityCompletedResponse("token1", "result")));
            Assert.IsFalse(new ActivityCompletedResponse("token", null).Equals(new ActivityCompletedResponse("token1", "result")));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new ActivityCompletedResponse(null, "result"));
        }

        [Test]
        public async Task Send_activity_completed_response_to_amazon_swf()
        {
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityCompletedResponse("token", "result");
            var cancellationTokenSource = new CancellationTokenSource();

            await response.SendAsync(simpleWorkflow.Object, cancellationTokenSource.Token);

            Func<RespondActivityTaskCompletedRequest, bool> request = r =>
            {
                Assert.That(r.TaskToken, Is.EqualTo("token"));
                Assert.That(r.Result, Is.EqualTo("result"));
                return true;
            };
            simpleWorkflow.Verify(s => s.RespondActivityTaskCompletedAsync(It.Is<RespondActivityTaskCompletedRequest>(r => request(r)), cancellationTokenSource.Token), Times.Once);
        }
    }
}