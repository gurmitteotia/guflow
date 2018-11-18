// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerCancelledEventTests
    {
        private TimerCancelledEvent _timerCancelledEvent;
        private const string _timerName ="timer name";
        private readonly TimeSpan _fireAfter = TimeSpan.FromSeconds(2);
        private EventGraphBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder(); 
            _timerCancelledEvent = CreateTimerCancelledEvent(Identity.Timer(_timerName),_fireAfter);
        }
        [Test]
        public void Should_not_be_active()
        {
            Assert.IsFalse(_timerCancelledEvent.IsActive);
        }
        private TimerCancelledEvent CreateTimerCancelledEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerCancelledEventGraph = _builder.TimerCancelledGraph(identity.ScheduleId(), fireAfter);
            return new TimerCancelledEvent(timerCancelledEventGraph.First(),timerCancelledEventGraph);
        }
    }
}