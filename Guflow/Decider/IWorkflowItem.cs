// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;

namespace Guflow.Decider
{
    /// <summary>
    /// Represents a schedulable workflow item- timer, activity etc.
    /// </summary>
    public interface IWorkflowItem
    {
        /// <summary>
        /// Returns all parent activities.
        /// </summary>
        IEnumerable<IActivityItem> ParentActivities { get; }
        /// <summary>
        /// Returns all parent timers.
        /// </summary>
        IEnumerable<ITimerItem> ParentTimers{ get; }

        /// <summary>
        /// Return latest event for workflow item. Returns null when no event is found.
        /// </summary>
        WorkflowItemEvent LastEvent { get; }

        /// <summary>
        /// Returns all events 
        /// </summary>
        IEnumerable<WorkflowItemEvent> AllEvents { get; }

        /// <summary>
        /// Returns true if workflow item is active otherwise false is return.
        /// </summary>
        bool IsActive { get; }
    }

    internal interface ITimer
    {
        WorkflowAction Fired(TimerFiredEvent timerFiredEvent);
        WorkflowAction Cancelled(TimerCancelledEvent timerCancelledEvent);
        WorkflowAction StartFailed(TimerStartFailedEvent timerStartFailedEvent);
        WorkflowAction CancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent);
    }
    internal interface IActivity
    {
        WorkflowAction Completed(ActivityCompletedEvent activityCompletedEvent);
        WorkflowAction Failed(ActivityFailedEvent activityFailedEvent);
        WorkflowAction Timedout(ActivityTimedoutEvent activityTimedoutEvent);
        WorkflowAction Cancelled(ActivityCancelledEvent activityCancelledEvent);
        WorkflowAction CancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent);
        WorkflowAction SchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent);
    }
}