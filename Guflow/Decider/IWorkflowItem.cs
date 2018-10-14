﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
        IEnumerable<ITimerItem> ParentTimers { get; }


        /// <summary>
        /// Returns all parent lambda functions.
        /// </summary>
        IEnumerable<ILambdaItem> ParentLambdas { get; }

        /// <summary>
        /// Returns all the parent child workflows.
        /// </summary>
        IEnumerable<IChildWorkflowItem> ParentChildWorkflows { get; }

            /// <summary>
        /// Return latest event for workflow item. Returns null when no event is found.
        /// </summary>
        /// <param name="includeRescheduleTimerEvents">Pass true if want to return reschedule timer event, if any, associated with this workflow item.</param>
        WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false);

        /// <summary>
        /// Returns all events 
        /// </summary>
        /// <param name="includeRescheduleTimerEvents">Pass true if want to return reschedule timer events, if any, associated with this workflow item.</param>
        IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false);

        /// <summary>
        /// Returns true if workflow item is active otherwise false is return.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns the last similar events for this item. e.g. if an activity has following events (starting with latest): ActivityCompletedEvent, ActivityCompletedEvent
        /// ActivityFailedEvent and ActivityFailedEvent. Then this api will return the last two ActivityCompletedEvents.
        /// </summary>
        /// <returns></returns>
        IEnumerable<WorkflowItemEvent> LastSimilarEvents();
    }

    internal interface ITimer
    {
        WorkflowAction Fired(TimerFiredEvent timerFiredEvent);
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