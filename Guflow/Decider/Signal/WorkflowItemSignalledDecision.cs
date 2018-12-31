// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow.Model;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;

namespace Guflow.Decider
{
    internal class WorkflowItemSignalledDecision : WorkflowDecision
    {
        private readonly ScheduleId _id;
        private readonly long _triggerEventId;
        private readonly string _signalName;

        public WorkflowItemSignalledDecision(ScheduleId id, long triggerEventId, string signalName) : base(false)
        {
            _id = id;
            _triggerEventId = triggerEventId;
            _signalName = signalName;
        }

        internal override Decision SwfDecision()
        {
            var details = new WorkflowItemSignalledData()
            {
                ScheduleId = _id,
                TriggerEventId = _triggerEventId,
                SignalName = _signalName
            };
            return new Decision
            {
                DecisionType = DecisionType.RecordMarker,
                RecordMarkerDecisionAttributes = new RecordMarkerDecisionAttributes()
                {
                    MarkerName = InternalMarkerNames.WorkflowItemSignalled,
                    Details = details.ToJson()
                }
            };
        }

        public override bool Equals(object obj)
        {
            return obj is WorkflowItemSignalledDecision decision &&
                   EqualityComparer<ScheduleId>.Default.Equals(_id, decision._id) &&
                   _triggerEventId == decision._triggerEventId &&
                   string.Equals(_signalName, decision._signalName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            var hashCode = -2131934865;
            hashCode = hashCode * -1521134295 + EqualityComparer<ScheduleId>.Default.GetHashCode(_id);
            hashCode = hashCode * -1521134295 + _triggerEventId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_signalName.ToLower());
            return hashCode;
        }
    }
}