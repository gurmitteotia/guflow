// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;

namespace Guflow.Tests
{
    internal class HistoryEventsBuilder
    {
        private readonly List<HistoryEvent> _processedEvents = new List<HistoryEvent>();
        private readonly List<HistoryEvent> _newEvents = new List<HistoryEvent>();
        public HistoryEventsBuilder AddProcessedEvents(params HistoryEvent[] events)
        {
            _processedEvents.InsertRange(0, events);
            return this;
        }

        public HistoryEventsBuilder AddNewEvents(params HistoryEvent[] events)
        {
            _newEvents.InsertRange(0, events);
            return this;
        }

        public WorkflowHistoryEvents Result()
        {
            var totalEvents = _newEvents.Concat(_processedEvents);
            return new WorkflowHistoryEvents(totalEvents, _newEvents.Last().EventId, _newEvents.First().EventId);
        }
    }
}