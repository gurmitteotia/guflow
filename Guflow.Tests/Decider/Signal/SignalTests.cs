// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalTests
    {
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
        }
        [Test]
        public void Invalid_arguments_test()
        {
            var signalAction = new Signal("name", "input");
            Assert.Throws<ArgumentException>(() => signalAction.ForWorkflow(null, "runid"));
            Assert.Throws<ArgumentNullException>(() => signalAction.ReplyTo(null));
        }
        [Test]
        public void Returns_signal_workflow_action()
        {
            var signalAction = new Signal("name", "input");

            var decisions = signalAction.ForWorkflow("id", "runid").Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new SignalWorkflowDecision("name", "input", "id", "runid")}));
        }

        [Test]
        public void Replying_to_a_signal_returns_signal_workflow_workflow()
        {
            var receivedSignalEvent = new WorkflowSignaledEvent(_builder.WorkflowSignaledEvent("someName","input1","rid","wid"));
            var signalAction = new Signal("name", "input");

            var decisions = signalAction.ReplyTo(receivedSignalEvent).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new SignalWorkflowDecision("name", "input", "wid", "rid")}));
        }
    }
}