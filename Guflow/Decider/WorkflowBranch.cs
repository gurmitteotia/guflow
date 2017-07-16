using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
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

        public BorkflowBranchJoint FindFirstJoint()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class BorkflowBranchJoint
    {
    }
}