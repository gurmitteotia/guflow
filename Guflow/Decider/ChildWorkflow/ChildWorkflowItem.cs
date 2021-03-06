﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class ChildWorkflowItem : WorkflowItem, IFluentChildWorkflowItem, IChildWorkflowItem
    {
        private Func<ChildWorkflowCompletedEvent, WorkflowAction> _completedAction;
        private Func<ChildWorkflowFailedEvent, WorkflowAction> _failedAction;
        private Func<ChildWorkflowCancelledEvent, WorkflowAction> _cancelledAction;
        private Func<ChildWorkflowTerminatedEvent, WorkflowAction> _terminatedAction;
        private Func<ChildWorkflowTimedoutEvent, WorkflowAction> _timedoutAction;
        private Func<ChildWorkflowStartFailedEvent, WorkflowAction> _startFailedAction;
        private Func<ExternalWorkflowCancelRequestFailedEvent, WorkflowAction> _cancelRequestFailedAction;
        private Func<ChildWorkflowStartedEvent, WorkflowAction> _startedAction;
        private Func<IChildWorkflowItem, object> _input;
        private Func<IChildWorkflowItem, string> _childPolicy;
        private Func<IChildWorkflowItem, int?> _taskPriority;
        private Func<IChildWorkflowItem, string> _lambdaRole;
        private Func<IChildWorkflowItem, string> _taskListName;
        private Func<IChildWorkflowItem, WorkflowTimeouts> _timeouts;
        private Func<IChildWorkflowItem, IEnumerable<string>> _tags;
        private Func<IChildWorkflowItem, bool> _when;
        private Func<IChildWorkflowItem, WorkflowAction> _onWhenFalseAction;
        
        public ChildWorkflowItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
            Initialize(workflow);
            _childPolicy = _ => null;
            _taskPriority = _ => null;
            _lambdaRole = _ => null;
            _taskListName = _ => null;
            _timeouts = _ => new WorkflowTimeouts();

        }
        public ChildWorkflowItem(Identity identity, IWorkflow workflow, WorkflowDescription desc) : base(identity, workflow)
        {
            Initialize(workflow);
            _childPolicy = w => desc.DefaultChildPolicy;
            _taskPriority = w => desc.DefaultTaskPriority;
            _lambdaRole = w => desc.DefaultLambdaRole;
            _taskListName = w => desc.DefaultTaskListName;
            _timeouts = _ => new WorkflowTimeouts()
            {
                ExecutionStartToCloseTimeout = desc.DefaultExecutionStartToCloseTimeout,
                TaskStartToCloseTimeout = desc.DefaultTaskStartToCloseTimeout
            };
        }

        private void Initialize(IWorkflow workflow)
        {
            _completedAction = w => w.DefaultAction(workflow);
            _failedAction = w => w.DefaultAction(workflow);
            _cancelledAction = w => w.DefaultAction(workflow);
            _terminatedAction = w => w.DefaultAction(workflow);
            _timedoutAction = w => w.DefaultAction(workflow);
            _startFailedAction = w => w.DefaultAction(workflow);
            _startedAction = _ => WorkflowAction.Empty;
            _input = w => workflow.WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _tags = _ => Enumerable.Empty<string>();
            _when = _ => true;
            _onWhenFalseAction = _ => IsStartupItem() ? WorkflowAction.Empty : new TriggerActions(this).FirstJoint();
            _cancelRequestFailedAction = e => e.DefaultAction(workflow);
        }
       
        public string Version => Identity.Version;
        public string PositionalName => Identity.PositionalName;
        protected override ScheduleId ScheduleId=> Identity.ScheduleId(WorkflowHistoryEvents.WorkflowRunId);

        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
        {
            var lastEvent = WorkflowHistoryEvents.LastChildWorkflowEvent(this);
            WorkflowItemEvent timerEvent = null;
            if (includeRescheduleTimerEvents)
                timerEvent = WorkflowHistoryEvents.LastTimerEvent(RescheduleTimer, true);

            if (lastEvent > timerEvent) return lastEvent;
            return timerEvent;
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false)
        {
            var childWorkflowItems = WorkflowHistoryEvents.AllChildWorkflowEvents(this);
            var timerEvents = Enumerable.Empty<WorkflowItemEvent>();
            if (includeRescheduleTimerEvents)
                timerEvents = WorkflowHistoryEvents.AllTimerEvents(RescheduleTimer, true);
            return childWorkflowItems.Concat(timerEvents).OrderByDescending(i => i, WorkflowEvent.IdComparer);
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisions()
        {
            if (!_when(this))
                return WorkflowDecisionsOnFalseWhen(_onWhenFalseAction(this));

            return ScheduleDecisionsByIgnoringWhen();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen()
        {
            return new[] {new ScheduleChildWorkflowDecision(ScheduleId, _input(this))
            {
                ChildPolicy = _childPolicy(this),
                TaskPriority = _taskPriority(this),
                LambdaRole = _lambdaRole(this),
                TaskListName = _taskListName(this),
                ExecutionTimeouts = _timeouts(this),
                Tags = _tags(this).ToArray()
            }};
        }

        public override IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout)
        {
            var timer = RescheduleTimer;
            timer.FireAfter(timeout);
            return timer.ScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> CancelDecisions()
        {
            var lastEvent = LastEvent(true);
            var latestTimerEvent = WorkflowHistoryEvents.LastTimerEvent(RescheduleTimer, true);
            if (latestTimerEvent != null && lastEvent == latestTimerEvent)
                return RescheduleTimer.CancelDecisions();

            return new[] { new CancelRequestWorkflowDecision(ScheduleId, (lastEvent as ChildWorkflowEvent)?.RunId), };
        }
        public IFluentChildWorkflowItem AfterTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Timer(name));
            return this;
        }

        public IFluentChildWorkflowItem AfterActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentChildWorkflowItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var desc = ActivityDescription.FindOn<TActivity>();
            return AfterActivity(desc.Name, desc.Version, positionalName);
        }

        public IFluentChildWorkflowItem AfterLambda(string name, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Lambda(name, positionalName));
            return this;
        }

        public IFluentChildWorkflowItem AfterChildWorkflow(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentChildWorkflowItem AfterChildWorkflow<TWorkflow>(string positionalName) where TWorkflow : Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return AfterChildWorkflow(desc.Name, desc.Version, positionalName);
        }

        public IFluentChildWorkflowItem WithInput(Func<IChildWorkflowItem, object> input)
        {
            Ensure.NotNull(input, nameof(input));
            _input = input;
            return this;
        }

        public IFluentChildWorkflowItem WithChildPolicy(Func<IChildWorkflowItem, string> childPolicy)
        {
            Ensure.NotNull(childPolicy, nameof(childPolicy));
            _childPolicy = childPolicy;
            return this;
        }

        public IFluentChildWorkflowItem WithPriority(Func<IChildWorkflowItem, int?> priority)
        {
            Ensure.NotNull(priority, nameof(priority));
            _taskPriority = priority;
            return this;
        }

        public IFluentChildWorkflowItem OnTaskList(Func<IChildWorkflowItem, string> name)
        {
            Ensure.NotNull(name, nameof(name));
            _taskListName = name;
            return this;
        }

        public IFluentChildWorkflowItem WithLambdaRole(Func<IChildWorkflowItem, string> lambdaRole)
        {
            Ensure.NotNull(lambdaRole, nameof(lambdaRole));
            _lambdaRole = lambdaRole;
            return this;
        }

        public IFluentChildWorkflowItem WithTimeouts(Func<IChildWorkflowItem, WorkflowTimeouts> timeouts)
        {
            Ensure.NotNull(timeouts, nameof(timeouts));
            _timeouts = timeouts;
            return this;
        }

        public IFluentChildWorkflowItem WithTags(Func<IChildWorkflowItem, IEnumerable<string>> tags)
        {
            Ensure.NotNull(tags, nameof(tags));
            _tags = tags;
            return this;
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

        public IFluentChildWorkflowItem When(Func<IChildWorkflowItem, bool> @true)
        {
            Ensure.NotNull(@true, nameof(@true));
            _when = @true;
            return this;
        }

        public IFluentChildWorkflowItem When(Func<IChildWorkflowItem, bool> @true, Func<IChildWorkflowItem, WorkflowAction> falseAction)
        {
            Ensure.NotNull(@true, nameof(@true));
            Ensure.NotNull(falseAction, nameof(falseAction));
            _when = @true;
            _onWhenFalseAction = falseAction;
            return this;
        }

        public IFluentChildWorkflowItem OnCancellationFailed(Func<ExternalWorkflowCancelRequestFailedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _cancelRequestFailedAction = workflowAction;
            return this;
        }

        public IFluentChildWorkflowItem OnStarted(Func<ChildWorkflowStartedEvent, WorkflowAction> workflowAction)
        {
            Ensure.NotNull(workflowAction, nameof(workflowAction));
            _startedAction = workflowAction;
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
      
        public WorkflowAction SignalAction(string signalName, string input)
        {
            var lastEvent = LastEvent() as ChildWorkflowEvent;
            return WorkflowAction.Signal(signalName, input, ScheduleId, lastEvent?.RunId);
        }

        public WorkflowAction CancelRequestFailedAction(ExternalWorkflowCancelRequestFailedEvent @event)
        {
            return _cancelRequestFailedAction(@event);
        }

        public WorkflowAction StartedAction(ChildWorkflowStartedEvent startedEvent)
        {
            return _startedAction(startedEvent);
        }
    }
}