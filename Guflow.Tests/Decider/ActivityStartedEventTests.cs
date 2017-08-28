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
        private const string _activityName = "Download";
        private const string _activityVersion = "1.0";
        private const string _positionalName = "First";
        private const string _workerId = "id";
        private ActivityStartedEvent _activityStartedEvent;

        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var scheduledActivityEventGraph = _builder.ActivityStartedGraph(Identity.New(_activityName, _activityVersion, _positionalName),_workerId);
            _activityStartedEvent = new ActivityStartedEvent(scheduledActivityEventGraph.First(), scheduledActivityEventGraph);
        }
        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activityStartedEvent.WorkerIdentity,Is.EqualTo(_workerId));
            Assert.That(_activityStartedEvent.IsActive,Is.True);
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _activityStartedEvent.Interpret(new EmptyWorkflow()));
        }
    }
}