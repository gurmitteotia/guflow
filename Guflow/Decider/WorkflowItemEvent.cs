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
        protected AwsIdentity AwsIdentity;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(AwsIdentity);
        }

        protected WorkflowItemEvent(long eventId)
            : base(eventId)
        {
        }

        internal static readonly WorkflowItemEvent NotFound = new NotFoundEvent();

        /// <summary>
        /// Indicate if this is an active event.
        /// </summary>
        public bool IsActive { get; protected set; }

        internal virtual bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            return false;
        }

        internal bool IsForSameWorkflowItemAs(WorkflowItemEvent otherWorkflowItemEvent)
        {
            return AwsIdentity == otherWorkflowItemEvent.AwsIdentity;
        }

        private class NotFoundEvent : WorkflowItemEvent
        {
            public NotFoundEvent() : base(-1)
            {
                AwsIdentity = AwsIdentity.Raw(Guid.NewGuid().ToString());
            }
        }
    }
}