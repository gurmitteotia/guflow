// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class ChildWorkflowItem : WorkflowItem, IFluentChildWorkflowItem
    {
        private Func<ChildWorkflowCompletedEvent, WorkflowAction> _completedAction;
        private Func<ChildWorkflowFailedEvent, WorkflowAction> _failedAction;
        private Func<ChildWorkflowCancelledEvent, WorkflowAction> _cancelledAction;
        private Func<ChildWorkflowTerminatedEvent, WorkflowAction> _terminatedAction;
        private Func<ChildWorkflowTimedoutEvent, WorkflowAction> _timedoutAction;
        private Func<ChildWorkflowStartFailedEvent, WorkflowAction> _startFailedAction;

        public ChildWorkflowItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
            _completedAction = w => w.DefaultAction(workflow);
            _failedAction = w => w.DefaultAction(workflow);
            _cancelledAction = w => w.DefaultAction(workflow);
            _terminatedAction = w => w.DefaultAction(workflow);
            _timedoutAction = w => w.DefaultAction(workflow);
            _startFailedAction = w => w.DefaultAction(workflow);
        }

        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisions()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowDecision> CancelDecisions()
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem AfterTimer(string name)
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem AfterActivity(string name, string version, string positionalName = "")
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem AfterLambda(string name, string positionalName = "")
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem AfterChildWorkflow(string name, string version, string positionalName = "")
        {
            throw new NotImplementedException();
        }

        public IFluentChildWorkflowItem OnCompletion(Func<ChildWorkflowCompletedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _completedAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnFailure(Func<ChildWorkflowFailedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _failedAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnCancelled(Func<ChildWorkflowCancelledEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _cancelledAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnTerminated(Func<ChildWorkflowTerminatedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _terminatedAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnTimedout(Func<ChildWorkflowTimedoutEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _timedoutAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnStartFailed(Func<ChildWorkflowStartFailedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _startFailedAction = workflowAction;
            return this;
        }

        public WorkflowAction CompletedAction(ChildWorkflowCompletedEvent completedEvent)
        {
            return _completedAction(completedEvent);
        }

        public WorkflowAction FailedAction(ChildWorkflowFailedEvent failedEvent)
        {
            return _failedAction(failedEvent);
        }

        public WorkflowAction CancelledAction(ChildWorkflowCancelledEvent cancelledEvent)
        {
            return _cancelledAction(cancelledEvent);
        }

        public WorkflowAction TerminatedAction(ChildWorkflowTerminatedEvent terminatedEvent)
        {
            return _terminatedAction(terminatedEvent);
        }

        public WorkflowAction TimedoutAction(ChildWorkflowTimedoutEvent timedoutEvent)
        {
            return _timedoutAction(timedoutEvent);
        }

        public WorkflowAction StartFailed(ChildWorkflowStartFailedEvent startFailed)
        {
            return _startFailedAction(startFailed);
        }
    }
}