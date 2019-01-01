﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the event of a scheduleable item like- activity, timer etc.
    /// </summary>
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        protected ScheduleId ScheduleId;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(ScheduleId);
        }

        protected WorkflowItemEvent(long eventId)
            : base(eventId)
        {
        }
        /// <summary>
        /// Indicate if this is an active event.
        /// </summary>
        public bool IsActive { get; protected set; }

        internal virtual bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            return false;
        }

        internal bool HasSameScheduleId(WorkflowItemEvent other) => ScheduleId == other.ScheduleId;
        /// <summary>
        /// Wait for the signal indefintely
        /// </summary>
        /// <param name="signalName">Signal name. Cases are ignored when comparing the signal names.</param>
        /// <returns></returns>
        public WaitForSignalsWorkflowAction WaitForSignal(string signalName)
        {
            return new WaitForSignalsWorkflowAction(ScheduleId, EventId, signalName);
        }
    }
}