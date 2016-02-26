using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerFiredEvent :WorkflowItemEvent
    {
        private readonly IEnumerable<HistoryEvent> _allHistoryEvents;
        private readonly TimerFiredEventAttributes _eventAttributes;
        private Identity _timerIdentity;
        private bool _isATimeoutTimer;
        public TimerFiredEvent(HistoryEvent timerFiredEvent, IEnumerable<HistoryEvent> allHistoryEvents)
        {
            _allHistoryEvents = allHistoryEvents;
            _eventAttributes =timerFiredEvent.TimerFiredEventAttributes;
            PopulateProperties(allHistoryEvents);
        }

        public override WorkflowAction Interpret(IWorkflow workflow)
        {
            return workflow.TimerFired(this);
        }

        public override IWorkflowContext WorkflowContext
        {
            get { return new WorkflowContext(_allHistoryEvents); }
        }

        public string Name { get { return _eventAttributes.TimerId; } }
        public TimeSpan FiredAfter { get; private set; }

        private Identity Identity
        {
            get
            {
                return Identity.Timer(Name);
            }
        }

        internal override bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(Identity);
        }

        private void PopulateProperties(IEnumerable<HistoryEvent> allHistoryEvents)
        {
            foreach (var historyEvent in allHistoryEvents)
            {
                if (historyEvent.IsTimerStartedEventFor(_eventAttributes.StartedEventId))
                {
                    FiredAfter = TimeSpan.FromSeconds(int.Parse(historyEvent.TimerStartedEventAttributes.StartToFireTimeout));
                }
            }
        }
    }
}