// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        public IEnumerable<ILambdaItem> ParentLambdas => _parentItems.OfType<ILambdaItem>();

        public IEnumerable<IChildWorkflowItem> ParentChildWorkflows => _parentItems.OfType<IChildWorkflowItem>();
        public string Name => Identity.Name;

        public bool IsActive
        {
            get
            {
                var lastEvent = LastEvent(true);
                return lastEvent != null && lastEvent.IsActive;
            }
        }

        public IEnumerable<WorkflowItemEvent> LastSimilarEvents()
        {
            WorkflowItemEvent lastEvent = null;
            foreach (var @event in AllEvents())
            {
                lastEvent = lastEvent ?? @event;
                if (lastEvent.GetType() == @event.GetType())
                    yield return @event;
                else
                    yield break;
            }
        }

        public bool IsWaitingForSignal(string signalName) => WaitForSignalsEvent(signalName) != null;

        public WorkflowAction Resume(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            var waitEvent = WaitForSignalsEvent(signalName);
            if (waitEvent == null)
                throw new SignalResumeException($"Workflow item {Identity} is not waiting for signal {signalName}");
            
            return WorkflowAction.ContinueWorkflow(this) + WorkflowAction.Custom(waitEvent.RecordSignal(signalName, SignalEventId()));
        }

        public bool IsSignalled(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            var waitEvent = _workflow.WaitForSignalsEvents.Reverse().FirstOrDefault(this);
            if (waitEvent == null) return false;
            return waitEvent.HasReceivedSignal(signalName);
        }

        public abstract WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false);
        public abstract IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false);

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

        public abstract IEnumerable<WorkflowDecision> ScheduleDecisions();

        public abstract IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen();
        public abstract IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout);
        public abstract IEnumerable<WorkflowDecision> CancelDecisions();
        public abstract bool Has(ScheduleId id);

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
            var parentBranches = ParentBranches().Where(p => !p.Has(exceptBranchOf)).ToArray();
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
            var lastEvent = LastEvent(true);
            if (lastEvent == null || lastEvent.IsActive)
                return false;
            var lastEventAction = lastEvent.Interpret(_workflow);
            return lastEventAction.ReadyToScheduleChildren;
        }
        public bool CanScheduleAny(IEnumerable<WorkflowItem> workflowItems)
        {
            var lastEvent = LastEvent(true);
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
        protected TimerItem RescheduleTimer(ScheduleId identity) => TimerItem.Reschedule(this, identity, _workflow);
        public WorkflowAction DefaultActionOnLastEvent()
        {
            return LastEvent().DefaultAction(_workflow);
        }

        private WaitForSignalsEvent WaitForSignalsEvent(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return _workflow.WaitForSignalsEvents.FirstOrDefault(this, signalName);
        }

        private long SignalEventId()
        {
            var e = _workflow.CurrentlyExecutingEvent as WorkflowSignaledEvent;
            return e != null ? e.EventId : 0;
        }
    }
}
