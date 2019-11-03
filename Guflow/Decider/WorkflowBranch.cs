// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    //TODO: Refactor this class.
    internal class WorkflowBranch
    {
        private readonly List<WorkflowItem> _workflowItems = new List<WorkflowItem>();
        private readonly IWorkflow _workflow;
        private WorkflowBranch(IWorkflow workflow, params WorkflowItem[] branchItems)
        {
            _workflow = workflow;
            _workflowItems.AddRange(branchItems);
        }

        public static IEnumerable<WorkflowBranch> ParentBranches(WorkflowItem startItem, IWorkflow workflow)
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

        public static IEnumerable<WorkflowBranch> ChildBranches(WorkflowItem startItem, IWorkflow workflow)
        {
            var allBranches = new List<WorkflowBranch>();

            var childBranch = new WorkflowBranch(workflow, startItem);
            if (childBranch.Children().Any())
                foreach (var child in childBranch.Children())
                    allBranches.Add(childBranch.Add(child));
            else
                allBranches.Add(childBranch);

            return allBranches;
        }

        private IEnumerable<WorkflowBranch> Parents()
        {
            return _workflowItems.Last().Parents()
                .SelectMany(p=>ParentBranches(p,_workflow));
        }

        private IEnumerable<WorkflowBranch> Children()
        {
            return _workflowItems.Last().Children()
                .SelectMany(i=>ChildBranches(i,_workflow));
        }

        private WorkflowBranch Add(WorkflowBranch workflowBranch)
        {
            return new WorkflowBranch(_workflow, _workflowItems.Concat(workflowBranch._workflowItems).ToArray());
        }

        public bool IsActive(IEnumerable<WorkflowBranch> parentBranches)
        {
            var lastWorkflowEvents = _workflowItems.Select(w => w.LastEvent(true));
            var sortedLastEvents = lastWorkflowEvents.OrderByDescending(e => e, WorkflowEvent.IdComparer).ToArray();
            if (sortedLastEvents.Any(e => e!=null && e.IsActive))
                return true;
            if (sortedLastEvents.All(e => e == null))
                return false;
            var latestEvent = sortedLastEvents.First();

            var latestEventItem = _workflowItems.First(i => latestEvent.IsFor(i));
            var action = latestEvent.Interpret(_workflow);
            var triggeredAction = action.TriggeredAction(latestEventItem);

            var immediateParent = _workflowItems.First();
            if (latestEvent.IsFor(immediateParent) && triggeredAction.ReadyToScheduleChildren)
                return false;

            var parentItems = parentBranches.SelectMany(b => b._workflowItems);
            return triggeredAction.CanScheduleAny(_workflow,parentItems);
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