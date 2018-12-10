// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;

namespace Guflow.Decider
{
    public class WaitForSignalWorkflowAction : WorkflowAction
    {
        private readonly ScheduleId _scheduleId;
        private readonly long _eventId;
        private readonly string _signalName;

        internal WaitForSignalWorkflowAction(ScheduleId scheduleId, long eventId, string signalName)
        {
            _scheduleId = scheduleId;
            _eventId = eventId;
            _signalName = signalName;
        }

        internal override IEnumerable<WorkflowDecision> Decisions()
        {
            return new[] {new WaitForSignalDecision(_scheduleId, _eventId, _signalName)};
        }
    }
}