using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    internal abstract class WorkflowItem : IWorkflowItem
    {
        private readonly IWorkflow _workflow;
        private readonly HashSet<WorkflowItem> _parentItems = new HashSet<WorkflowItem>();
        protected readonly Identity Identity;
        protected WorkflowItem(Identity identity, IWorkflow workflow)
        {
            Identity = identity;
            _workflow = workflow;
        }
        public IEnumerable<IActivityItem> ParentActivities => _parentItems.OfType<IActivityItem>();

        public IEnumerable<ITimerItem> ParentTimers => _parentItems.OfType<ITimerItem>();

        public string Name => Identity.Name;

        public bool IsActive
        {
            get
            {
                var lastEvent = LastEvent;
                return lastEvent != null && lastEvent.IsActive;
            }
        }

        public abstract WorkflowItemEvent LastEvent { get; }
        public abstract IEnumerable<WorkflowItemEvent> AllEvents { get; }
        public bool IsStartupItem()
        {
            return _parentItems.Count == 0;
        }
        public bool IsChildOf(WorkflowItem workflowItem)
        {
            return _parentItems.Contains(workflowItem);
        }

        public IEnumerable<WorkflowItem> Children()
        {
            return _workflow.GetChildernOf(this);
        }

        public IEnumerable<WorkflowItem> Parents()
        {
            return _parentItems;
        }

        public abstract IEnumerable<WorkflowDecision> GetScheduleDecisions();
        public abstract IEnumerable<WorkflowDecision> GetRescheduleDecisions(TimeSpan afterTimeout);
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

        public override string ToString()
        {
            return Identity.ToString();
        }

        public bool AreAllParentBranchesInactive(WorkflowItem exceptBranchOf)
        {
            var parentBranches = ParentBranches().Where(p=>!p.Has(exceptBranchOf)).ToArray();
            foreach (var parentBranch in parentBranches)
            {
                if (parentBranch.IsActive(parentBranches))
                    return false;
            }
            return true;
        }

        public IEnumerable<WorkflowBranch> ParentBranches()
        {
            return _parentItems.SelectMany(WorkflowBranch.ParentBranches);
        }

        public IEnumerable<WorkflowBranch> ChildBranches()
        {
            return WorkflowBranch.ChildBranches(this);
        }

        public bool IsReadyToScheduleChildren()
        {
            var lastEvent = LastEvent;
            if (lastEvent == null || lastEvent.IsActive)
                return false;
            var lastEventAction = lastEvent.Interpret(_workflow);
            return lastEventAction.ReadyToScheduleChildren;
        }
        public bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            var lastEvent = LastEvent;
            if (lastEvent == null)
                return false;
            if (lastEvent.IsActive)
                return true;
            var lastEventAction = lastEvent.Interpret(_workflow);
            return lastEventAction.CanScheduleAny(workflowItems);
        }
        protected void AddParent(Identity identity)
        {
            var parentItem = _workflow.FindWorkflowItemBy(identity);
            if (parentItem == null)
                throw new ParentItemMissingException(string.Format(Resources.Schedulable_item_missing, identity));
            if (Equals(parentItem))
                throw new CyclicDependencyException(string.Format(Resources.Cyclic_dependency, identity));
            _parentItems.Add(parentItem);
        }

        protected IWorkflowHistoryEvents WorkflowHistoryEvents => _workflow.WorkflowHistoryEvents;

        public WorkflowAction DefaultActionOnLastEvent()
        {
            return LastEvent.DefaultAction(_workflow);
        }
    }
}
