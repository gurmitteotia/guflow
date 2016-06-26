﻿using System;
using System.Collections.Generic;

namespace Guflow
{
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

        public bool IsActive { get; protected set; }
        public static readonly WorkflowItemEvent NotFound = new NotFoundWorkflowItemEvent(0);

        internal virtual bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            return false;
        }

        internal bool IsForSameWorkflowItemAs(WorkflowItemEvent otherWorkflowItemEvent)
        {
            return AwsIdentity == otherWorkflowItemEvent.AwsIdentity;
        }
        private class NotFoundWorkflowItemEvent : WorkflowItemEvent
        {
            public NotFoundWorkflowItemEvent(long eventId) : base(eventId)
            {
                AwsIdentity= AwsIdentity.Raw("");
            }
            internal override WorkflowAction Interpret(IWorkflow workflow)
            {
                throw new NotSupportedException("Can not interpret not found event.");
            }
        }
    }
}