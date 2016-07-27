using System;
using System.Collections.Generic;

namespace Guflow.Tests
{
    internal class WorkflowToStubHistoryEvents : IWorkflow
    {
        public WorkflowToStubHistoryEvents(IWorkflowHistoryEvents workflowHistoryEvents)
        {
            CurrentHistoryEvents = workflowHistoryEvents;
        }
        public IEnumerable<WorkflowItem> GetStartupWorkflowItems()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item)
        {
            throw new System.NotImplementedException();
        }

        public WorkflowItem Find(Identity identity)
        {
            throw new System.NotImplementedException();
        }

        public IWorkflowHistoryEvents CurrentHistoryEvents { get; private set; }

        public WorkflowAction OnWorkflowStarted(WorkflowStartedEvent workflowStartedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivityCompletion(ActivityCompletedEvent activityCompletedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivityFailure(ActivityFailedEvent activityFailedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivityTimeout(ActivityTimedoutEvent activityTimedoutEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivityCancelled(ActivityCancelledEvent activityCancelledEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnTimerFired(TimerFiredEvent timerFiredEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnTimerStartFailure(TimerStartFailedEvent timerStartFailedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnTimerCancelled(TimerCancelledEvent timerCancelledEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivityCancellationFailed(ActivityCancellationFailedEvent activityCancellationFailedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnTimerCancellationFailed(TimerCancellationFailedEvent timerCancellationFailedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnActivitySchedulingFailed(ActivitySchedulingFailedEvent activitySchedulingFailedEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnWorkflowSignaled(WorkflowSignaledEvent workflowSignaledEvent)
        {
            throw new NotImplementedException();
        }

        public WorkflowAction OnWorkflowCancellationRequested(WorkflowCancellationRequestedEvent workflowCancellationRequestedEvent)
        {
            throw new NotImplementedException();
        }
    }
}