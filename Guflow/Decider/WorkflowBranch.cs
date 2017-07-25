using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowBranch
    {
        private readonly List<WorkflowItem> _workflowItems = new List<WorkflowItem>();

        private WorkflowBranch(params WorkflowItem[] branchItems)
        {
            _workflowItems.AddRange(branchItems);
        }

        public static IEnumerable<WorkflowBranch> Build(WorkflowItem startItem)
        {
            var allBranches = new List<WorkflowBranch>();

            var parentBranch = new WorkflowBranch(startItem);
            if (parentBranch.Parents().Any())
                foreach (var parent in parentBranch.Parents())
                    allBranches.Add(parentBranch.Add(parent));
            else
                allBranches.Add(parentBranch);

            return allBranches;
        }

        private IEnumerable<WorkflowBranch> Parents()
        {
            return _workflowItems.Last().Parents()
                .SelectMany(Build);
        }

        private WorkflowBranch Add(WorkflowBranch workflowBranch)
        {
            return new WorkflowBranch(_workflowItems.Concat(workflowBranch._workflowItems).ToArray());
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

        public BorkflowBranchJoint FindFirstJoint()
        {
            throw new System.NotImplementedException();
        }

        public bool Has(WorkflowItem workflowItem)
        {
            return _workflowItems.Contains(workflowItem);
        }
    }

    internal class BorkflowBranchJoint
    {
    }
}