// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivityScheduledEventTests
    {
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private ActivityScheduledEvent _activityScheduledEvent;

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var scheduledActivityEventGraph = _builder.ActivityScheduledGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId());
            _activityScheduledEvent = new ActivityScheduledEvent(scheduledActivityEventGraph.First(),scheduledActivityEventGraph);
        }

        [Test]
        public void Should_populate_attributes()
        {
            Assert.That(_activityScheduledEvent.IsActive,Is.True);
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            var workflow = new EmptyWorkflow();

            Assert.That(()=>_activityScheduledEvent.Interpret(workflow), Throws.TypeOf<NotSupportedException>());
        }
    }
}