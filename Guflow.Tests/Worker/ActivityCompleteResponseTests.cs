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
            Assert.IsTrue(new ActivityCompleteResponse("token", "result").Equals(new ActivityCompleteResponse("token", "result")));
            Assert.IsTrue(new ActivityCompleteResponse("token", null).Equals(new ActivityCompleteResponse("token", null)));


            Assert.IsFalse(new ActivityCompleteResponse("token", "result").Equals(new ActivityCompleteResponse("token", "result1")));
            Assert.IsFalse(new ActivityCompleteResponse("token", "result").Equals(new ActivityCompleteResponse("token1", "result")));
            Assert.IsFalse(new ActivityCompleteResponse("token", null).Equals(new ActivityCompleteResponse("token1", "result")));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new ActivityCompleteResponse(null, "result"));
        }

        [Test]
        public async Task Send_activity_completed_response_to_amazon_swf()
        {
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityCompleteResponse("token", "result");
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