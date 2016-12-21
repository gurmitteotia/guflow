using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelRequestTests
    {
        private CancelRequest _cancelRequest;

        [SetUp]
        public void Setup()
        {
            _cancelRequest = new CancelRequest(null);
        }
        [Test]
        public void Invalid_arugments_test()
        {
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForActivity("activity", null));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForTimer( null));
            Assert.Throws<ArgumentException>(() => _cancelRequest.ForWorkflow(null));
            Assert.Throws<ArgumentNullException>(() => _cancelRequest.For(null));
        }
    }
}