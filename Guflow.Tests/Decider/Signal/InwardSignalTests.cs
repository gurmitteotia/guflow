// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class InwardSignalTests
    {
        private const string SignalName = "name";
        private InwardSignal _signal;
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Mock<IWorkflow> _workflow;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _workflow = new Mock<IWorkflow>();
            _signal = new InwardSignal(SignalName, _workflow.Object);
        }

        [Test]
        public void Is_triggered_true()
        {
            var signalEvent = new WorkflowSignaledEvent(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.CurrentlyExecutingEvent).Returns(signalEvent);

            Assert.That(_signal.IsTriggered(), Is.True);
        }

        [Test]
        public void Is_triggered_false()
        {
            var signalEvent = new WorkflowSignaledEvent(_graphBuilder.WorkflowSignaledEvent("Diff".ToUpper(), "input"));
            _workflow.SetupGet(w => w.CurrentlyExecutingEvent).Returns(signalEvent);

            Assert.That(_signal.IsTriggered(), Is.False);
        }

        [Test]
        public void Is_triggered_true_with_data()
        {
            var signalEvent = new WorkflowSignaledEvent(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.CurrentlyExecutingEvent).Returns(signalEvent);

            Assert.That(_signal.IsTriggered(d=>d=="input"), Is.True);
        }

        [Test]
        public void Is_triggered_false_data()
        {
            var signalEvent = new WorkflowSignaledEvent(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.CurrentlyExecutingEvent).Returns(signalEvent);

            Assert.That(_signal.IsTriggered(d=> d== "data"), Is.False);
        }

        [Test]
        public void Is_received_true()
        {
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());

            Assert.That(_signal.IsReceived(), Is.True);
        }

        [Test]
        public void Is_received_false()
        {
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Diff".ToUpper(), "input"));
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());

            Assert.That(_signal.IsReceived(), Is.False);
        }

        [Test]
        public void Is_received_true_with_data()
        {
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());

            Assert.That(_signal.IsReceived(d=>d=="input"), Is.True);
        }

        [Test]
        public void Is_received_false_data()
        {
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent(SignalName.ToUpper(), "input"));
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());

            Assert.That(_signal.IsReceived(d => d == "diff"), Is.False);
        }
    }
}