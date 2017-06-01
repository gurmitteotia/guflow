using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        //public bool SchedulingIsAllowedByAllParents()
        //{
        //    return _parentItems.All(p => p.AllowSchedulingOfChildWorkflowItem());
        //}

        public bool AreAllParentBranchesInactive(WorkflowItem exceptBranchOf)
        {
            var parentsItems = _parentItems.Except(new[] { exceptBranchOf });
            foreach (var parentsItem in parentsItems)
            {
                var parentBranches = WorkflowItemsBranch.BuildParentBranchStartingWith(parentsItem, _workflow).ToArray();
                if (!parentBranches.All(p => p.IsInactive(parentBranches)))
                    return false;

            }
            return true;
        }

        //private IEnumerable<WorkflowItem> AllParentItemsInBranchOf(WorkflowItem workflowItem)
        //{
        //    var list = new List<WorkflowItem> {workflowItem};

        //    var parentWorkflowItems = _workflow.GetParentsOf(workflowItem);
        //    foreach (var parentWorkflowItem in parentWorkflowItems)
        //    {
        //        list.AddRange(AllParentItemsInBranchOf(parentWorkflowItem));
        //    }

        //    return list;
        //}

        public bool AllowSchedulingOfChildWorkflowItem()
        {
            var lastEvent = LastEvent;
            if (lastEvent != WorkflowItemEvent.NotFound && !lastEvent.IsActive)
            {
                var lastEventAction = lastEvent.Interpret(_workflow);
                return lastEventAction.AllowSchedulingOfChildWorkflowItem();
            }

            return false;
        }

        public bool IsLastEventTriggerSchedulingOfAny(IEnumerable<WorkflowItem> workflowItems)
        {
            var lastEvent = LastEvent;
            if (lastEvent != WorkflowItemEvent.NotFound && !lastEvent.IsActive)
            {
                var lastEventDecisions = lastEvent.Interpret(_workflow).GetDecisions();
                return lastEventDecisions.Any(d => workflowItems.Any(d.IsFor));
            }
            return false;
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

    internal class WorkflowItemsBranch
    {
        private readonly IWorkflow _workflow;
        private readonly List<WorkflowItem> _workflowItems = new List<WorkflowItem>();

        private WorkflowItemsBranch(IWorkflow workflow, params WorkflowItem[] workflowItems)
        {
            _workflow = workflow;
            _workflowItems.AddRange(workflowItems);
        }

        public static IEnumerable<WorkflowItemsBranch> BuildParentBranchStartingWith(WorkflowItem startItem, IWorkflow workflow)
        {
            var allBranches = new List<WorkflowItemsBranch>();

            var parentBranch = new WorkflowItemsBranch(workflow, startItem);
            if (parentBranch.Parents().Any())
                foreach (var parent in parentBranch.Parents())
                    allBranches.Add(parentBranch.Add(parent));
            else
                allBranches.Add(parentBranch);

            return allBranches;
        }

        private IEnumerable<WorkflowItemsBranch> Parents()
        {
            return _workflow.GetParentsOf(_workflowItems.Last())
                    .SelectMany(p => BuildParentBranchStartingWith(p, _workflow));
        }

        private WorkflowItemsBranch Add(WorkflowItemsBranch workflowItemsBranch)
        {
            return new WorkflowItemsBranch(_workflow, _workflowItems.Concat(workflowItemsBranch._workflowItems).ToArray());
        }

        public bool IsInactive(IEnumerable<WorkflowItemsBranch> allBranches)
        {
            var immediateParent = _workflowItems[0];

            if (immediateParent.IsActive)
                return false;
            if (immediateParent.AllowSchedulingOfChildWorkflowItem())
                return true;

            var allItemsInBranches = allBranches.SelectMany(b => b._workflowItems);
            
            return _workflowItems.Skip(1).All(w => !w.IsActive && !w.IsLastEventTriggerSchedulingOfAny(allItemsInBranches));
        }
     
    }
}
