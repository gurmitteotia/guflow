using System;

namespace Guflow
{
    public sealed class ActivityItem : WorkflowItem
    {
        private Func<ActivityCompletedEvent, WorkflowAction> _onCompletionAction;
        private Func<ActivityFailedEvent, WorkflowAction> _onFailedAction;
        private Func<ActivityTimedoutEvent, WorkflowAction> _onTimedoutAction;
        private Func<ActivityCancelledEvent, WorkflowAction> _onCancelledAction;
        private Func<ActivityCancellationFailedEvent, WorkflowAction> _onFailedCancellationAction; 
        
        internal ActivityItem(string name, string version, string positionalName, IWorkflowItems workflowItems)
            : base(Identity.New(name, version, positionalName), workflowItems)
        {
            _onCompletionAction = c => new ContinueWorkflowAction(this, c.WorkflowHistoryEvents);
            _onFailedAction = c => WorkflowAction.FailWorkflow(c.Reason, c.Detail);
            _onTimedoutAction = t => WorkflowAction.FailWorkflow(t.TimeoutType, t.Details);
            _onCancelledAction = c => WorkflowAction.CancelWorkflow(c.Details);
            _onFailedCancellationAction = c => WorkflowAction.FailWorkflow("ACTIVITY_CANCELLATION_FAILED", c.Cause);
        }

        public ActivityItem DependsOn(string name, string version, string positionalName = "")
        {
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public ActivityItem DependsOn(string timerName)
        {
            AddParent(Identity.Timer(timerName));
            return this;
        }
       
        internal override WorkflowDecision GetScheduleDecision()
        {
            return new ScheduleActivityDecision(Identity);
        }

        internal override WorkflowDecision GetCancelDecision()
        {
            return new CancelActivityDecision(Identity);
        }

        internal override WorkflowAction TimerFired(TimerFiredEvent timerFiredEvent)
        {
            return WorkflowAction.Schedule(this);
        }

        internal WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent)
        {
            return _onCompletionAction(activityCompletedEvent);
        }

        protected override bool IsProcessed(IWorkflowHistoryEvents workflowHistoryEvents)
        {
            var activity = workflowHistoryEvents.LatestActivityEventFor(this);
            return activity != null;
        }

        public ActivityItem OnCompletion(Func<ActivityCompletedEvent, WorkflowAction> onCompletionFunc)
        {
            _onCompletionAction = onCompletionFunc;
            return this;
        }

        internal WorkflowAction Failed(ActivityFailedEvent activityFailedEvent)
        {
            return _onFailedAction(activityFailedEvent);
        }

        public ActivityItem OnFailure(Func<ActivityFailedEvent, WorkflowAction> onFailureFunc)
        {
            _onFailedAction = onFailureFunc;
            return this;
        }

        internal WorkflowAction Timedout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            return _onTimedoutAction(activityTimedoutEvent);
        }

        public ActivityItem OnTimedout(Func<ActivityTimedoutEvent, WorkflowAction> onTimedoutFunc)
        {
            _onTimedoutAction = onTimedoutFunc;
            return this;
        }

        internal WorkflowAction Cancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            return _onCancelledAction(activityCancelledEvent);
        }

        public ActivityItem OnCancelled(Func<ActivityCancelledEvent, WorkflowAction> onCancelledFunc)
        {
            _onCancelledAction = onCancelledFunc;
            return this;
        }

        public ActivityItem OnTimerCancelled(Func<TimerCancelledEvent, WorkflowAction> onTimerCancelledAction)
        {
            OnTimerCancelledAction = onTimerCancelledAction;
            return this;
        }

        internal WorkflowAction CancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            return _onFailedCancellationAction(activityCancellationFailedEvent);
        }
        public ActivityItem OnFailedCancellation(Func<ActivityCancellationFailedEvent, WorkflowAction> onFailedCancellationAction)
        {
            _onFailedCancellationAction = onFailedCancellationAction;
            return this;
        }
    }
}