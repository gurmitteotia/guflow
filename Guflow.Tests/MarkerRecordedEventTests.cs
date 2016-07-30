using System;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class MarkerRecordedEventTests
    {
        private MarkerRecordedEvent _markerRecordedEvent;

        [SetUp]
        public void Setup()
        {
            _markerRecordedEvent = new MarkerRecordedEvent(HistoryEventFactory.CreateMarkerRecordedEvent("name1","detail1"));
        }
        [Test]
        public void Populate_properties_from_event_attributes()
        {
            Assert.That(_markerRecordedEvent.MarkerName,Is.EqualTo("name1"));
            Assert.That(_markerRecordedEvent.Details, Is.EqualTo("detail1"));
        }

        [Test]
        public void Throws_exception_when_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _markerRecordedEvent.Interpret(new Mock<IWorkflowActions>().Object));
        }
    }
}