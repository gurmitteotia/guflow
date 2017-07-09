using System;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    public class MarkerRecordedEvent : WorkflowEvent
    {
        private readonly MarkerRecordedEventAttributes _eventAttributes;
        internal MarkerRecordedEvent(HistoryEvent markerRecordedEvent) : base(markerRecordedEvent.EventId)
        {
            _eventAttributes = markerRecordedEvent.MarkerRecordedEventAttributes;
        }

        public string MarkerName { get { return _eventAttributes.MarkerName; } }
        public string Details { get { return _eventAttributes.Details; }}
    }
}