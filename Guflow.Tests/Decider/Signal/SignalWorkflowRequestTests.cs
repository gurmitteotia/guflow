// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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

        [Test]
        public void Serialize_signal_input_to_json_format()
        {
            var req = new SignalWorkflowRequest("id", "name");
            req.SignalInput = new {Id = 10};

            var swfRequest = req.SwfFormat("d");

            Assert.That(swfRequest.Input, Is.EqualTo("{\"Id\":10}"));
        }
    }
}