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

        [SetUp]
        public void Setup()
        {
            var scheduledActivityEventGraph = HistoryEventFactory.CreateActivityScheduledEventGraph(Identity.New(_activityName, _activityVersion, _positionalName));
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