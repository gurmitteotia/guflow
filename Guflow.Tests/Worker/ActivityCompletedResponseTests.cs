using System;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.Tests.Worker
{
   [TestFixture]
    public class ActivityCompletedResponseTests
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
    }
}