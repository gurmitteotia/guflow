// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

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
        private Func<IChildWorkflowItem, object> _input;
        private Func<IChildWorkflowItem, string> _childPolicy;
        private Func<IChildWorkflowItem, int?> _taskPriority;
        private Func<IChildWorkflowItem, string> _lambdaRole;
        private Func<IChildWorkflowItem, string> _taskListName;
        private Func<IChildWorkflowItem, WorkflowTimeouts> _timeouts;
        private Func<IChildWorkflowItem, IEnumerable<string>> _tags;

        public ChildWorkflowItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
           Initialize(workflow);
            _childPolicy = _ => null;
            _taskPriority = _ => null;
            _lambdaRole = _ => null;
            _taskListName = _ => null;
            _timeouts = _=> new WorkflowTimeouts();

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
            _input = w => workflow.WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _tags = _=> Enumerable.Empty<string>();
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
            return new[] {new ScheduleChildWorkflowDecision(Identity, _input(this))
            {
                ChildPolicy = _childPolicy(this),
                TaskPriority = _taskPriority(this),
                LambdaRole = _lambdaRole(this),
                TaskListName = _taskListName(this),
                ExecutionTimeouts = _timeouts(this),
                Tags = _tags(this).ToArray()
            }};
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