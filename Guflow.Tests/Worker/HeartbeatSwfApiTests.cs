// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

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
    public class HeartbeatSwfApiTests
    {
        private HeartbeatSwfApi _api;
        private Mock<IAmazonSimpleWorkflow> _client;

        [SetUp]
        public void Setup()
        {
            _client = new Mock<IAmazonSimpleWorkflow>();
            _api = new HeartbeatSwfApi(_client.Object);
        }

        [Test]
        public async Task Returns_true_cancel_status_for_heartbeat()
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus() {CancelRequested = true}
            };
            _client.Setup(c => c.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var cancelled = await _api.RecordHearbeatAsync("token", "detail", new CancellationTokenSource().Token);

            Assert.IsTrue(cancelled);
        }

        [Test]
        public async Task Returns_false_cancel_status_for_heartbeat()
        {
            var response = new RecordActivityTaskHeartbeatResponse()
            {
                ActivityTaskStatus = new ActivityTaskStatus() { CancelRequested = false }
            };
            _client.Setup(c => c.RecordActivityTaskHeartbeatAsync(It.IsAny<RecordActivityTaskHeartbeatRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var cancelled = await _api.RecordHearbeatAsync("token", "detail", new CancellationTokenSource().Token);

            Assert.IsFalse(cancelled);
        }
    }
}