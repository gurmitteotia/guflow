﻿using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;

namespace Guflow
{
    public class TimerStartedEvent : TimerEvent
    {
        public TimerStartedEvent(HistoryEvent timerStartedEvent, IEnumerable<HistoryEvent> allHistoryEvents):base(timerStartedEvent.EventId)
        {
            PopulateProperties(timerStartedEvent.EventId,allHistoryEvents);
            IsActive = true;
        }

        internal override WorkflowAction Interpret(IWorkflow workflow)
        {
            throw new NotSupportedException("Can not interpret timer started event.");

        }
    }
}