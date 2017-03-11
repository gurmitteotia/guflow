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
            Assert.IsTrue(new ActivityCancelResponse("token", "details").Equals(new ActivityCancelResponse("token", "details")));
            Assert.IsTrue(new ActivityCancelResponse("token", null).Equals(new ActivityCancelResponse("token", null)));

            Assert.IsFalse(new ActivityCancelResponse("token", "details").Equals(new ActivityCancelResponse("token", "details1")));
            Assert.IsFalse(new ActivityCancelResponse("token", "details").Equals(new ActivityCancelResponse("token", null)));
        }

        [Test]
        public void Invalid_argument_test()
        {
            Assert.Throws<ArgumentException>(() => new ActivityCancelResponse(null, "detail"));
        }

        [Test]
        public async Task Send_cancel_response_to_amazon_swf()
        {
            var simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            var response = new ActivityCancelResponse("token", "details");
            var cancellationTokenSource = new CancellationTokenSource();

            await response.SendAsync(simpleWorkflow.Object, cancellationTokenSource.Token);

            Func<RespondActivityTaskCanceledRequest, bool> request = r =>
            {
                Assert.That(r.TaskToken, Is.EqualTo("token"));
                Assert.That(r.Details, Is.EqualTo("details"));
                return true;
            };
            simpleWorkflow.Verify(s => s.RespondActivityTaskCanceledAsync(It.Is<RespondActivityTaskCanceledRequest>(r => request(r)), cancellationTokenSource.Token), Times.Once);
        }
    }
}