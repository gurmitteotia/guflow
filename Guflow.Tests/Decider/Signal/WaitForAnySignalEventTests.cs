using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForAnySignalEventTests
    {
        private EventGraphBuilder _graphBuilder;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
        }

        [Test]
        public void No_more_waits_when_first_signal_is_received()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] {"e1", "e2"}, SignalWaitType.Any);
            var r = _graphBuilder.SignalResumedEvent(ScheduleId.Raw("id"), 10, "e1");

            var @event = new WaitForSignalsEvent(w, new []{r, w});

            Assert.That(@event.WaitingSignals, Is.Empty);
            Assert.That(@event.IsExpectingSignal, Is.False);
        }


        [Test]
        public void No_more_waits_when_second_signal_is_received()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.Any);
            var r = _graphBuilder.SignalResumedEvent(ScheduleId.Raw("id"), 10, "e2");

            var @event = new WaitForSignalsEvent(w, new[] { r, w });

            Assert.That(@event.WaitingSignals, Is.Empty);
            Assert.That(@event.IsExpectingSignal, Is.False);
        }

        [Test]
        public void Keep_waiting_when_resumed_signal_is_for_differnt_schedule_id()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.Any);
            var r = _graphBuilder.SignalResumedEvent(ScheduleId.Raw("id1"), 10, "e2");

            var @event = new WaitForSignalsEvent(w, new[] { r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[] { "e1", "e2" }));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }

        [Test]
        public void Keep_waiting_when_resumed_signal_is_for_differnt_trigger_event_id()
        {
            var w = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"), 10, new[] { "e1", "e2" }, SignalWaitType.Any);
            var r = _graphBuilder.SignalResumedEvent(ScheduleId.Raw("id"), 11, "e2");

            var @event = new WaitForSignalsEvent(w, new[] { r, w });

            Assert.That(@event.WaitingSignals, Is.EqualTo(new[] { "e1", "e2" }));
            Assert.That(@event.IsExpectingSignal, Is.True);
        }

    }
}