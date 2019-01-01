using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForAllSignalsEventTests
    {
        private EventGraphBuilder _graphBuilder;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
        }

        [Test]
        public void Keeps_waiting_when_first_signal_is_received()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.All);
            var r = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 10, "e1");

            var @event = new WaitForSignalsEvent(w, new[] { r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[]{"e2"}));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }

        [Test]
        public void Keeps_waiting_when_second_signal_is_received()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.All);
            var r = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 10, "e2");

            var @event = new WaitForSignalsEvent(w, new[] { r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[] { "e1" }));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }


        [Test]
        public void No_more_waits_when_both_signals_are_received()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.All);
            var r = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 10, "e2");
            var r1 = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 10, "e1");

            var @event = new WaitForSignalsEvent(w, new[] {r1, r, w });

            Assert.That(@event.WaitingSignals, Is.Empty);
            Assert.That(@event.IsExpectingSignal, Is.False);
        }

        [Test]
        public void Keep_waiting_when_resumed_signals_are_for_differnt_schedule_id()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.All);
            var r = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id1"), 10, "e2");
            var r1 = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id1"), 10, "e1");

            var @event = new WaitForSignalsEvent(w, new[] {r1, r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[] { "e1", "e2" }));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }

        [Test]
        public void Keep_waiting_when_resumed_signals_are_for_differnt_trigger_event_id()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.All);
            var r = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 11, "e2");
            var r1 = _graphBuilder.WorkflowItemSignalledEvent(ScheduleId.Raw("id"), 11, "e1");

            var @event = new WaitForSignalsEvent(w, new[] {r1, r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[] { "e1", "e2" }));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }
    }
}