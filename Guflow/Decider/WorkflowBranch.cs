// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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

        public static IEnumerable<WorkflowBranch> ParentBranches(WorkflowItem startItem)
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

        public static IEnumerable<WorkflowBranch> ChildBranches(WorkflowItem startItem)
        {
            var allBranches = new List<WorkflowBranch>();

            var childBranch = new WorkflowBranch(startItem);
            if (childBranch.Childs().Any())
                foreach (var child in childBranch.Childs())
                    allBranches.Add(childBranch.Add(child));
            else
                allBranches.Add(childBranch);

            return allBranches;
        }

        private IEnumerable<WorkflowBranch> Parents()
        {
            return _workflowItems.Last().Parents()
                .SelectMany(ParentBranches);
        }

        private IEnumerable<WorkflowBranch> Childs()
        {
            return _workflowItems.Last().Children()
                .SelectMany(ChildBranches);
        }

        private WorkflowBranch Add(WorkflowBranch workflowBranch)
        {
            return new WorkflowBranch(_workflowItems.Concat(workflowBranch._workflowItems).ToArray());
        }

        public bool IsActive(IEnumerable<WorkflowBranch> parentBranches)
        {
            var lastWorkflowEvents = _workflowItems.Select(w => w.LastEvent);
            var sortedLastEvents = lastWorkflowEvents.OrderByDescending(e => e, WorkflowEvent.IdComparer);
            if (sortedLastEvents.Any(e => e!=null && e.IsActive))
                return true;
            if (sortedLastEvents.All(e => e == null))
                return false;
            var immediateParent = _workflowItems.First();
            var latestEvent = sortedLastEvents.First();
            if (latestEvent.IsFor(immediateParent) && immediateParent.IsReadyToScheduleChildren())
                return false;

            var latestEventItem = _workflowItems.First(i => latestEvent.IsFor(i));
            var parentItems = parentBranches.SelectMany(b => b._workflowItems);

            return latestEventItem.CanScheduleAny(parentItems);
        }

        public WorkflowItem FindFirstJointItem(WorkflowItem beforeItem)
        {
            if (Has(beforeItem))
                return _workflowItems.TakeWhile(i => !i.Equals(beforeItem))
                        .FirstOrDefault(i => i.Parents().Count() > 1);

            return null;
        }

        public WorkflowItem FindFirstJointItem()
        {
            return _workflowItems.FirstOrDefault(i => i.Parents().Count() > 1);
        }

        public bool Has(WorkflowItem workflowItem)
        {
            return _workflowItems.Contains(workflowItem);
        }
    }


}