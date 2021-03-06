﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Properties;

namespace Guflow.Decider
{
    internal abstract class WorkflowItem : IWorkflowItem, ITimer
    {
        protected readonly IWorkflow Workflow;
        private readonly HashSet<WorkflowItem> _parentItems = new HashSet<WorkflowItem>();
        protected readonly Identity Identity;
        private readonly Stack<WorkflowItem> _continueItems = new Stack<WorkflowItem>();
        private ScheduleId _scheduleId = null;
        private TimerItem _rescheduleTimer = null;
        protected WorkflowItem(Identity identity, IWorkflow workflow)
        {
            Identity = identity;
            Workflow = workflow;
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

        public WorkflowAction Resume(WorkflowSignaledEvent signal)
        {
            Ensure.NotNull(signal, nameof(signal));
            var waitEvent = WaitForSignalsEvent(signal.SignalName);
            if (waitEvent == null)
                throw new SignalResumeException($"Workflow item {Identity} is not waiting for signal {signal.SignalName}");
            WorkflowDecision decision = null;
            if (waitEvent.HasTimedout(signal))
                decision = waitEvent.RecordTimedout(signal);
            else
                decision = waitEvent.RecordSignal(signal);

            var recordedAction = WorkflowAction.Custom(decision);

            if (waitEvent.IsExpectingSignals)
                return recordedAction;
            
            return waitEvent.NextAction(this) + recordedAction;
        }
        public bool IsWaitingForAnySignal()
        {
            return LatestWaitForSignals().Any();
        }
        public WorkflowAction SignalResumedAction()
        {
            var @event = LatestWaitForSignalsEvent();
            return @event.NextAction(this);
        }

        public bool IsSignalled(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            var waitEvent = LatestWaitForSignalsEvent();
            if (waitEvent == null) return false;
            return waitEvent.HasReceivedSignal(signalName);
        }

        public bool IsSignalTimedout(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            var waitEvent = LatestWaitForSignalsEvent();
            if (waitEvent == null) return false;
            return waitEvent.IsSignalTimedout(signalName);
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
            return Workflow.GetChildernOf(this);
        }

        public IEnumerable<WorkflowItem> Parents()
        {
            return _parentItems;
        }
        public abstract IEnumerable<WorkflowDecision> ScheduleDecisions();

        public abstract IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen();
        public abstract IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout);
        public abstract IEnumerable<WorkflowDecision> CancelDecisions();
        public virtual bool Has(ScheduleId id) => ScheduleId == id;

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
            return _parentItems.SelectMany(i=>WorkflowBranch.ParentBranches(i, Workflow));
        }

        public IEnumerable<WorkflowBranch> ChildBranches()
        {
            return Children().SelectMany(c=>WorkflowBranch.ChildBranches(c, Workflow));
        }

        protected virtual ScheduleId ScheduleId => _scheduleId??(_scheduleId=Identity.ScheduleId());
        protected virtual TimerItem RescheduleTimer =>  _rescheduleTimer??(_rescheduleTimer=TimerItem.Reschedule(this, ScheduleId, Workflow));

        protected void AddParent(Identity identity)
        {
            var parentItem = Workflow.WorkflowItem(identity);
            if (parentItem == null)
                throw new ParentItemMissingException(string.Format(Resources.Schedulable_item_missing, identity));
            if (Equals(parentItem))
                throw new CyclicDependencyException(string.Format(Resources.Cyclic_dependency, identity));
            _parentItems.Add(parentItem);
        }
        protected IWorkflowHistoryEvents WorkflowHistoryEvents => Workflow.WorkflowHistoryEvents;

        protected IEnumerable<WorkflowDecision> WorkflowDecisionsOnFalseWhen(WorkflowAction action) =>
            action.WithTriggeredItem(this).Decisions(Workflow);

        public WorkflowAction DefaultActionOnLastEvent()
        {
            return LastEvent().DefaultAction(Workflow);
        }

        private WaitForSignalsEvent WaitForSignalsEvent(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return Workflow.WaitForSignalsEvents.FirstOrDefault(this, signalName);
        }

        public WaitForSignalsEvent WaitForSignalsEvent(long triggerEventId)
        {
            return Workflow.WaitForSignalsEvents.FirstOrDefault(this,triggerEventId);
        }

        private WaitForSignalsEvent LatestWaitForSignalsEvent()
        {
            var signalEventsLatestFirst = Workflow.WaitForSignalsEvents.Reverse();
            return signalEventsLatestFirst.FirstOrDefault(this);
        }

        private IEnumerable<string> LatestWaitForSignals()
        {
            var @event = LatestWaitForSignalsEvent();
            if (@event == null) return Enumerable.Empty<string>();
            return @event.WaitingSignals;
        }

        public bool HasContinueItem(WorkflowItem item) => _continueItems.Contains(item);
        public void PushContinueItem(WorkflowItem item) => _continueItems.Push(item);
        public void PopContinueItem()=> _continueItems.Pop();
        public void ResetContinueItems() => _continueItems.Clear();

        public virtual WorkflowAction Fired(TimerFiredEvent timerFiredEvent)
        {
            if (timerFiredEvent.TimerType == TimerType.Reschedule)
            {
                ITimer timer = RescheduleTimer;
                return timer.Fired(timerFiredEvent);
            }
            if (timerFiredEvent.TimerType == TimerType.SignalTimer)
            {
                var waitForSignalEvent = WaitForSignalsEvent(timerFiredEvent.SignalTriggerEventId);
                if (!waitForSignalEvent.IsExpectingSignals) return WorkflowAction.Empty;

                var signalTimedoutDecision = waitForSignalEvent.RecordTimedout(timerFiredEvent);
                return WorkflowAction.Custom(signalTimedoutDecision) + WorkflowAction.ContinueWorkflow(this);
            }
            throw new InvalidOperationException("Timer fired should be called only for Reschedule and SignalTimer.");
        }

        public virtual WorkflowAction StartFailed(TimerStartFailedEvent timerStartFailedEvent)
        {
            ITimer timer = RescheduleTimer;
            return timer.StartFailed(timerStartFailedEvent);
        }


        public virtual WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            ITimer timer = RescheduleTimer;
            return timer.CancellationFailed(timerCancellationFailedEvent);
        }
    }
}
