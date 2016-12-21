using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerStartedEventTests
    {
        private const string _timerName = "timer";
        private TimerStartedEvent _timerStartedEvent;

        [SetUp]
        public void Setup()
        {
            _timerStartedEvent = CreateTimerStartedEvent(Identity.Timer(_timerName), TimeSpan.FromSeconds(2));
        }

        [Test]
        public void Should_be_active()
        {
            Assert.That(_timerStartedEvent.IsActive, Is.True);
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _timerStartedEvent.Interpret(new EmptyWorkflow()));
        }

        private TimerStartedEvent CreateTimerStartedEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(identity, fireAfter);
            return new TimerStartedEvent(timerFiredEventGraph.First(), timerFiredEventGraph);
        }
    }
}