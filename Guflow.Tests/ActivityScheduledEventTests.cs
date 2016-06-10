using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
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
            _activityScheduledEvent = new ActivityScheduledEvent(scheduledActivityEventGraph.First());
        }

        [Test]
        public void Can_not_be_interpreted()
        {
            var activityScheduledEvent = new ActivityScheduledEvent();
        }
    }
}