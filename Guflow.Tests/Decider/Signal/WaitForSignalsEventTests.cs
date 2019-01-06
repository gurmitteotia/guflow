// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForSignalsEventTests
    {
        private EventGraphBuilder _graphBuilder;
        private WaitForSignalsEvent _event;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            var graph = _graphBuilder.WaitForSignalEvent(ScheduleId.Raw("id"),10, new []{"e1", "e2"}, SignalWaitType.Any);
            _event = new WaitForSignalsEvent(graph, Enumerable.Empty<HistoryEvent>());
        }

        [Test]
        public void Populate_properties()
        {
            Assert.That(_event.TriggerEventId, Is.EqualTo(_event.TriggerEventId));
            Assert.That(_event.IsActive, Is.False);
            Assert.That(_event.WaitingSignals, Is.EqualTo(new []{"e1", "e2"}));
            Assert.That(_event.IsExpectingSignals, Is.True);

            Assert.IsTrue(_event.IsWaitingForSignal("e1"));
            Assert.IsTrue(_event.IsWaitingForSignal("E1"));
            Assert.IsFalse(_event.IsWaitingForSignal("E5"));
        }
        [Test]
        public void Throws_exception_when_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _event.Interpret(new EmptyWorkflow()));
        }
    }
}