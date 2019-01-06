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
        private string _workflowRunId;
        private string _workflowId;

        public HistoryEventsBuilder AddProcessedEvents(params HistoryEvent[] events)
        {
            _processedEvents.InsertRange(0, events);
            return this;
        }

        public HistoryEventsBuilder AddProcessedEvents(IEnumerable<HistoryEvent> events)
        {
            return AddProcessedEvents(events.ToArray());
        }

        public HistoryEventsBuilder AddNewEvents(params HistoryEvent[] events)
        {
            _newEvents.InsertRange(0, events);
            return this;
        }
        public HistoryEventsBuilder AddNewEvents(IEnumerable<HistoryEvent> events)
        {
            return AddNewEvents(events.ToArray());
        }

        public WorkflowHistoryEvents Result()
        {
            var totalEvents = _newEvents.Concat(_processedEvents).ToList();
            var decisionTask = new DecisionTask()
            {
                Events = totalEvents,
                WorkflowExecution = new WorkflowExecution() { RunId = _workflowRunId, WorkflowId = _workflowId}
            };
            if (_newEvents.Count > 0)
            {
                decisionTask.PreviousStartedEventId = _newEvents.Last().EventId - 1;
                decisionTask.StartedEventId = _newEvents.First().EventId;
            }
            return new WorkflowHistoryEvents(decisionTask);
        }

        public HistoryEventsBuilder AddWorkflowRunId(string runId)
        {
            _workflowRunId = runId;
            return this;
        }

        public HistoryEventsBuilder AddWorkflowId(string id)
        {
            _workflowId = id;
            return this;
        }
    }
}