using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowItemSignalTimedoutEvent : WorkflowItemEvent
    {
        private SignalsTimedoutDetails _details;
        public WorkflowItemSignalTimedoutEvent(HistoryEvent historyEvent) : base(historyEvent)
        {
            var attr = historyEvent.MarkerRecordedEventAttributes;
            _details = attr.Details.As<SignalsTimedoutDetails>();
            ScheduleId = ScheduleId.Raw(_details.ScheduleId);
        }

        public string Reason => _details.Reason;
        public IEnumerable<string> TimedoutTimedoutSignals => _details.TimedoutSignalNames;
        public bool IsFor(WaitForSignalsEvent @event)
        {
            return _details.TriggerEventId == @event.TriggerEventId
                   && HasSameScheduleId(@event);
        }
    }
}