// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowActions
    {
        private readonly List<WorkflowAction> _actions = new List<WorkflowAction>();
        private readonly List<WaitForSignalsEvent> _signalEvents = new List<WaitForSignalsEvent>();
        public void Add(WorkflowAction action) => _actions.Add(action);
        public WorkflowDecision[] CompatibleDecisions(Workflow workflow)
        {
            var decisions = _actions.Where(w => w != null).SelectMany(a => a.Decisions()).Distinct();
            return decisions.CompatibleDecisions(workflow).Where(d => d != WorkflowDecision.Empty).ToArray(); ;
        }

        public IEnumerable<WaitForSignalsEvent> WaitForSignalsEvents(Workflow workflow)
        {
            foreach (var decision in CompatibleDecisions(workflow).OfType<WaitForSignalsDecision>())
            {
                var @event = decision.WaitForSignalsEvent();
                if(!_signalEvents.Contains(@event))
                    _signalEvents.Add(@event);
            }
            return _signalEvents;
        }
    }
}