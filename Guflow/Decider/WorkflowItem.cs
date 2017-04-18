using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    internal abstract class  WorkflowItem : IWorkflowItem
    {
        private readonly IWorkflow _workflow;
        private readonly HashSet<WorkflowItem> _parentItems = new HashSet<WorkflowItem>();
        protected readonly Identity Identity;
        protected WorkflowItem(Identity identity, IWorkflow workflow)
        {
            Identity = identity;
            _workflow = workflow;
        }

        public IEnumerable<IActivityItem> ParentActivities { get { return _parentItems.OfType<IActivityItem>(); } }

        public IEnumerable<ITimerItem> ParentTimers { get { return _parentItems.OfType<ITimerItem>(); } }

        public string Name
        {
            get { return Identity.Name; }
        }

        public bool IsActive
        {
            get
            {
                var lastEvent = LastEvent;
                return lastEvent != WorkflowItemEvent.NotFound && lastEvent.IsActive;
            }
        }

        public abstract WorkflowItemEvent LastEvent { get; }

        public bool HasNoParents()
        {
            return _parentItems.Count == 0;
        }
        public bool IsChildOf(WorkflowItem workflowItem)
        {
            return _parentItems.Contains(workflowItem);
        }

        public IEnumerable<WorkflowItem> GetChildlern()
        {
            return _workflow.GetChildernOf(this);
        }

        public abstract WorkflowDecision GetScheduleDecision();
        public abstract WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout);
        public abstract WorkflowDecision GetCancelDecision();
        public bool Has(AwsIdentity identity)
        {
            return Identity.Id == identity;
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

        public bool SchedulingIsAllowedByAllParents()
        {
            return _parentItems.All(p => p.AllowSchedulingOfChildWorkflowItem());
        }
        private bool AllowSchedulingOfChildWorkflowItem()
        {
            var lastEvent = LastEvent;
            if (lastEvent != WorkflowItemEvent.NotFound && !lastEvent.IsActive)
            {
                var lastEventAction = lastEvent.Interpret(_workflow);
                return lastEventAction.AllowSchedulingOfChildWorkflowItem();
            }

            return false;
        }
        protected void AddParent(Identity identity)
        {
            var parentItem = _workflow.Find(identity);
            if (parentItem == null)
                throw new ParentItemMissingException(string.Format(Resources.Schedulable_item_missing, identity));
            if(Equals(parentItem))
                throw new CyclicDependencyException(string.Format(Resources.Cyclic_dependency, identity));
            _parentItems.Add(parentItem);
        }

        protected IWorkflowEvents WorkflowEvents
        {
            get { return _workflow.WorkflowEvents; }
        }
    }
}
