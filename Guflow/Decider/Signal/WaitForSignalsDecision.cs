// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Amazon.SimpleWorkflow.Model;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;

namespace Guflow.Decider
{
    internal class WaitForSignalsDecision: WorkflowDecision
    {
        private readonly WaitForSignalData _data;
        private readonly ScheduleId _id;
        public WaitForSignalsDecision(WaitForSignalData data) : base(false)
        {
            _data = data;
            _id= ScheduleId.Raw(data.ScheduleId);
        }

        internal override Decision SwfDecision()
        {
            return new Decision
            {
                DecisionType = DecisionType.RecordMarker,
                RecordMarkerDecisionAttributes = new RecordMarkerDecisionAttributes()
                {
                    MarkerName = InternalMarkerNames.WorkflowItemWaitForSignals,
                    Details = _data.ToJson()
                }
            };
        }

        internal override bool IsFor(WorkflowItem workflowItem)
            => workflowItem.Has(_id);

        public override bool Equals(object obj)
        {
            return obj is WaitForSignalsDecision decision &&
                   string.Equals(_data.ScheduleId, decision._data.ScheduleId) &&
                   _data.TriggerEventId == decision._data.TriggerEventId;
        }

        public override int GetHashCode()
        {
            var hashCode = -1394181897;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_data.ScheduleId);
            hashCode = hashCode * -1521134295 + _data.TriggerEventId.GetHashCode();
            return hashCode;
        }
    }
}