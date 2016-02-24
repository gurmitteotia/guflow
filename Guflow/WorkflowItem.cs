using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public abstract class WorkflowItem
    {
        private readonly IWorkflowItems _workflowItems;
        private readonly HashSet<WorkflowItem> _parentItems = new HashSet<WorkflowItem>();
        protected readonly Identity Identity;

        protected WorkflowItem(Identity identity, IWorkflowItems workflowItems)
        {
            Identity = identity;
            _workflowItems = workflowItems;
        }

        internal bool HasNoParents()
        {
            return _parentItems.Count == 0;
        }
        internal bool IsChildOf(WorkflowItem workflowItem)
        {
            return _parentItems.Contains(workflowItem);
        }

        internal IEnumerable<WorkflowItem> GetChildlern()
        {
            return _workflowItems.GetChildernOf(this);
        }

        internal abstract WorkflowDecision GetDecision();

        internal WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout)
        {
            return new ScheduleTimerDecision(Identity,afterTimeout);
        }

        public bool Has(Identity identity)
        {
            return Identity.Equals(identity);
        }
        public override bool Equals(object other)
        {
            var otherItem = other as WorkflowItem;
            if (otherItem == null)
                return false;
            return Identity.Equals(otherItem.Identity);
        }
        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        public bool AllParentsAreProcessed(IWorkflowContext workflowContext)
        {
            return _parentItems.All(p => p.IsProcessed(workflowContext));
        }

        protected abstract bool IsProcessed(IWorkflowContext workflowContext);

        protected void AddParent(Identity identity)
        {
            var parentItem = _workflowItems.Find(identity);
            if (parentItem == null)
                throw new ParentItemNotFoundException(string.Format("Can not find the schedulable item for {0}", identity));
            _parentItems.Add(parentItem);
        }
    }
}
