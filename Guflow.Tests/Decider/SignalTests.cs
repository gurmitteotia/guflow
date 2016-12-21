using System;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalTests
    {
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

            var workflowAction = signalAction.ForWorkflow("id", "runid");

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Signal("name", "input", "id", "runid")));
        }

        [Test]
        public void Replying_to_a_signal_returns_signal_workflow_workflow()
        {
            var receivedSignalEvent = new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("someName","input1","rid","wid"));
            var signalAction = new Signal("name", "input");

            var workflowAction = signalAction.ReplyTo(receivedSignalEvent);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.Signal("name", "input", "wid", "rid")));
        }
    }
}