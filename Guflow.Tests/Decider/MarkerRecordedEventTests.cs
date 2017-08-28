using System;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class MarkerRecordedEventTests
    {
        private MarkerRecordedEvent _markerRecordedEvent;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder(); 
            _markerRecordedEvent = new MarkerRecordedEvent(_builder.MarkerRecordedEvent("name1","detail1"));
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
            Assert.Throws<NotSupportedException>(() => _markerRecordedEvent.Interpret(new Mock<IWorkflow>().Object));
        }
    }
}