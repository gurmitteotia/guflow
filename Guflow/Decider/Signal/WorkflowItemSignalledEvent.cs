// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    internal class WorkflowItemSignalledEvent : WorkflowItemEvent
    {
        private readonly WorkflowItemSignalledData _data;
        public WorkflowItemSignalledEvent(HistoryEvent @event) : base(@event.EventId)
        {
            _data = @event.MarkerRecordedEventAttributes.Details.As<WorkflowItemSignalledData>();
            ScheduleId = ScheduleId.Raw(_data.ScheduleId);
        }

        public long TriggerEventId => _data.TriggerEventId;

        /// <summary>
        /// Returns the signal name.
        /// </summary>
        public string SignalName => _data.SignalName;

        internal bool IsFor(WaitForSignalsEvent waitEvent)
        {
            return TriggerEventId == waitEvent.TriggerEventId
                   && HasSameScheduleId(waitEvent);
        }
    }
}