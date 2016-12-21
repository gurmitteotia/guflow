using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalWorkflowRequestTests
    {
        [Test]
        public void Invalid_arguments_test()
        {
            Assert.Throws<ArgumentException>(() => new SignalWorkflowRequest("workflowId", null));
            Assert.Throws<ArgumentException>(() => new SignalWorkflowRequest("workflowId", ""));
            Assert.Throws<ArgumentException>(() => new SignalWorkflowRequest("", "name"));
            Assert.Throws<ArgumentException>(() => new SignalWorkflowRequest(null, "name"));
        }
         
    }
}