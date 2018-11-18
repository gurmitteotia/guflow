// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityStartedEventTests
    {
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string WorkerId = "id";
        private ActivityStartedEvent _activityStartedEvent;

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var scheduledActivityEventGraph = _builder.ActivityStartedGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId(),WorkerId);
            _activityStartedEvent = new ActivityStartedEvent(scheduledActivityEventGraph.First(), scheduledActivityEventGraph);
        }
        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activityStartedEvent.WorkerIdentity,Is.EqualTo(WorkerId));
            Assert.That(_activityStartedEvent.IsActive,Is.True);
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _activityStartedEvent.Interpret(new EmptyWorkflow()));
        }
    }
}