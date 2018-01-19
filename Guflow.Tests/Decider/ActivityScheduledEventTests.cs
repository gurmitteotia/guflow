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
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private ActivityScheduledEvent _activityScheduledEvent;

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var scheduledActivityEventGraph = _builder.ActivityScheduledGraph(Identity.New(_activityName, _activityVersion, _positionalName));
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