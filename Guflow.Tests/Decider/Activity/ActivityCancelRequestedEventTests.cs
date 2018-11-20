// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class ActivityCancelRequestedEventTests
    {
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string WorkerId = "id";
        private ActivityCancelRequestedEvent _activityCancelRequestedEvent;

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var scheduleId = Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId();
            var activityCancelRequestedGraph = _builder.ActivityCancelRequestedGraph(scheduleId,WorkerId);
            _activityCancelRequestedEvent = new ActivityCancelRequestedEvent(activityCancelRequestedGraph.First());
        }

        [Test]
        public void Should_populate_properties()
        {
            Assert.That(_activityCancelRequestedEvent.IsActive,Is.False);
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _activityCancelRequestedEvent.Interpret(new EmptyWorkflow()));
        }
    }
}