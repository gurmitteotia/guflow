// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Amazon.SimpleWorkflow.Model;
using System.Collections.Generic;
using Amazon.SimpleWorkflow;

namespace Guflow.Decider
{
    internal class WaitForSignalDecision: WorkflowDecision
    {
        private readonly ScheduleId _id;
        private readonly long _eventId;
        private readonly string _signalName;

        public WaitForSignalDecision(ScheduleId id, long eventId, string signalName) : base(false)
        {
            _id = id;
            _eventId = eventId;
            _signalName = signalName;
        }

        internal override Decision SwfDecision()
        {
            var details = new WaitForSignalScheduleData()
            {
                ScheduleId = _id,
                EventId = _eventId,
                EventNames = new[] {_signalName},
                WaitType = SignalWaitType.Any,
                NextAction = SignalNextAction.Continue
            };
            return new Decision()
            {
                DecisionType = DecisionType.RecordMarker,
                RecordMarkerDecisionAttributes = new RecordMarkerDecisionAttributes()
                {
                    MarkerName = InternalMarkerNames.WaitForSignal,
                    Details = details.ToJson()
                }
            };
        }

        public override bool Equals(object obj)
        {
            return obj is WaitForSignalDecision decision &&
                   EqualityComparer<ScheduleId>.Default.Equals(_id, decision._id) &&
                   _eventId == decision._eventId &&
                   string.Equals(_signalName, decision._signalName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            var hashCode = -1394181897;
            hashCode = hashCode * -1521134295 + EqualityComparer<ScheduleId>.Default.GetHashCode(_id);
            hashCode = hashCode * -1521134295 + _eventId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_signalName.ToLower());
            return hashCode;
        }
    }
}