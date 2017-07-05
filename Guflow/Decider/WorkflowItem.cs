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
        public virtual IEnumerable<WorkflowDecision> GetContinuedDecisions()
        {
            return new[] { GetScheduleDecision() };
        }
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

        public bool AreAllParentBranchesInactive(WorkflowItem exceptBranchOf)
        {
            var parentsItems = _parentItems.Except(new[] { exceptBranchOf });
            foreach (var parentsItem in parentsItems)
            {
                var parentBranches = WorkflowBranch.BuildParentBranchStartingWith(parentsItem, _workflow).ToArray();
                if (parentBranches.Any(p => p.IsActive(parentBranches)))
                    return false;
            }
            return true;
        }


        public bool IsReadyToScheduleChildren()
        {
            var lastEvent = LastEvent;
            if (lastEvent == WorkflowItemEvent.NotFound || lastEvent.IsActive)
                return false;
            var lastEventAction = lastEvent.Interpret(_workflow);
            return lastEventAction.ReadyToScheduleChildren;
        }
        public bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            var lastEvent = LastEvent;
            if (lastEvent == WorkflowItemEvent.NotFound)
                return false;
            if (lastEvent.IsActive)
                return true;
            var lastEventAction = lastEvent.Interpret(_workflow);
            return lastEventAction.CanScheduleAny(workflowItems);
        }
        protected void AddParent(Identity identity)
        {
            var parentItem = _workflow.Find(identity);
            if (parentItem == null)
                throw new ParentItemMissingException(string.Format(Resources.Schedulable_item_missing, identity));
            if (Equals(parentItem))
                throw new CyclicDependencyException(string.Format(Resources.Cyclic_dependency, identity));
            _parentItems.Add(parentItem);
        }

        protected IWorkflowEvents WorkflowEvents
        {
            get { return _workflow.WorkflowEvents; }
        }
    }

    internal class WorkflowBranch
    {
        private readonly IWorkflow _workflow;
        private readonly List<WorkflowItem> _workflowItems = new List<WorkflowItem>();

        private WorkflowBranch(IWorkflow workflow, params WorkflowItem[] workflowItems)
        {
            _workflow = workflow;
            _workflowItems.AddRange(workflowItems);
        }

        public static IEnumerable<WorkflowBranch> BuildParentBranchStartingWith(WorkflowItem startItem, IWorkflow workflow)
        {
            var allBranches = new List<WorkflowBranch>();

            var parentBranch = new WorkflowBranch(workflow, startItem);
            if (parentBranch.Parents().Any())
                foreach (var parent in parentBranch.Parents())
                    allBranches.Add(parentBranch.Add(parent));
            else
                allBranches.Add(parentBranch);

            return allBranches;
        }

        private IEnumerable<WorkflowBranch> Parents()
        {
            return _workflow.GetParentsOf(_workflowItems.Last())
                    .SelectMany(p => BuildParentBranchStartingWith(p, _workflow));
        }

        private WorkflowBranch Add(WorkflowBranch workflowBranch)
        {
            return new WorkflowBranch(_workflow, _workflowItems.Concat(workflowBranch._workflowItems).ToArray());
        }

        public bool IsActive(IEnumerable<WorkflowBranch> allBranches)
        {
            var lastWorkflowEvents = _workflowItems.Select(w => w.LastEvent);
            var sortedLastEvents = lastWorkflowEvents.OrderByDescending(e => e, WorkflowEvent.IdComparer);
            if (sortedLastEvents.Any(e => e.IsActive))
                return true;
            if (sortedLastEvents.All(e => e == WorkflowItemEvent.NotFound))
                return false;
            var immediateParent = _workflowItems.First();
            var latestEvent = sortedLastEvents.First();
            if (latestEvent.IsFor(immediateParent) && immediateParent.IsReadyToScheduleChildren())
                return false;

            var latestEventItem = _workflowItems.First(i => latestEvent.IsFor(i));
            var allItemsInBranches = allBranches.SelectMany(b => b._workflowItems);
            
            return latestEventItem.CanScheduleAny(allItemsInBranches);
        }
    }
}
