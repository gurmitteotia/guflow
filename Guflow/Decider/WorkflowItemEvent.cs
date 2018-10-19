// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the event of a scheduleable item like- activity, timer etc.
    /// </summary>
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        protected SwfIdentity SwfIdentity;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(SwfIdentity);
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
    }
}