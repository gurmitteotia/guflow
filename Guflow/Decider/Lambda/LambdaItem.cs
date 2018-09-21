// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class LambdaItem : WorkflowItem, IFluentLambdaItem, ILambdaItem
    {
        private Func<ILambdaItem, object> _input;
        private Func<ILambdaItem, TimeSpan?> _timeout;
        private Func<LambdaCompletedEvent, WorkflowAction> _completedAction;
        private Func<LambdaFailedEvent, WorkflowAction> _failedAction;
        private Func<LambdaTimedoutEvent, WorkflowAction> _timedoutAction;
        private Func<LambdaSchedulingFailedEvent, WorkflowAction> _schedulingFailedAction;
        private Func<LambdaStartFailedEvent, WorkflowAction> _startFailedAction;
        private Func<ILambdaItem, bool> _whenFunc = _ => true;
        private Func<ILambdaItem, WorkflowAction> _onFalseTrigger;
        private readonly TimerItem _rescheduleTimer;
        public LambdaItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
            _input = (item) => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _timeout = item => null;
            _rescheduleTimer = TimerItem.Reschedule(this, identity, workflow);
            _completedAction = e => e.DefaultAction(workflow);
            _failedAction = e => e.DefaultAction(workflow);
            _timedoutAction = e => e.DefaultAction(workflow);
            _schedulingFailedAction = e => e.DefaultAction(workflow);
            _startFailedAction = e => e.DefaultAction(workflow);
            _onFalseTrigger = _=> IsStartupItem()? WorkflowAction.Empty : new TriggerActions(this).FirstJoint();
        }

        public string PositionalName => Identity.PositionalName;

        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false)
        {
            var lambdaEvent = WorkflowHistoryEvents.LastLambdaEvent(this);
            WorkflowItemEvent timerEvent = null;
            if(includeRescheduleTimerEvents)
                timerEvent = WorkflowHistoryEvents.LastTimerEvent(_rescheduleTimer, true);

            if (lambdaEvent > timerEvent) return lambdaEvent;
            return timerEvent;
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false)
        {
            var lambdaEvents = WorkflowHistoryEvents.AllLambdaEvents(this);
            var timerEvents = Enumerable.Empty<WorkflowItemEvent>();
            if(includeRescheduleTimerEvents)
                timerEvents = WorkflowHistoryEvents.AllTimerEvents(_rescheduleTimer, true);

            return lambdaEvents.Concat(timerEvents).OrderByDescending(i => i, WorkflowEvent.IdComparer);
        }

        public override IEnumerable<WorkflowDecision> GetScheduleDecisions()
        {
            if (!_whenFunc(this))
                return _onFalseTrigger(this).Decisions();

            return new[] { new ScheduleLambdaDecision(Identity, _input(this), _timeout(this)) };
        }

        public override IEnumerable<WorkflowDecision> GetRescheduleDecisions(TimeSpan timeout)
        {
            _rescheduleTimer.FireAfter(timeout);
            return _rescheduleTimer.GetScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> GetCancelDecisions()
        {
            return Enumerable.Empty<WorkflowDecision>();
        }

        public IFluentLambdaItem AfterTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Timer(name));
            return this;
        }

        public IFluentLambdaItem AfterActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentLambdaItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            AddParent(Identity.New(description.Name, description.Version, positionalName));
            return this;
        }

        public IFluentLambdaItem AfterLambda(string name, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Lambda(name, positionalName));
            return this;
        }

        public IFluentLambdaItem WithInput(Func<ILambdaItem, object> input)
        {
            Ensure.NotNull(input, nameof(input));
            _input = input;
            return this;
        }

        public IFluentLambdaItem WithTimeout(Func<ILambdaItem, TimeSpan?> timout)
        {
            Ensure.NotNull(timout, nameof(timout));
            _timeout = timout;
            return this;
        }

        public IFluentLambdaItem OnCompletion(Func<LambdaCompletedEvent, WorkflowAction> completedAction)
        {
            Ensure.NotNull(completedAction, nameof(completedAction));
            _completedAction = completedAction;
            return this;
        }

        public IFluentLambdaItem OnFailure(Func<LambdaFailedEvent, WorkflowAction> failedAction)
        {
            Ensure.NotNull(failedAction, nameof(failedAction));
            _failedAction = failedAction;
            return this;
        }

        public IFluentLambdaItem OnTimedout(Func<LambdaTimedoutEvent, WorkflowAction> timedoutAction)
        {
            Ensure.NotNull(timedoutAction, nameof(timedoutAction));
            _timedoutAction = timedoutAction;
            return this;
        }

        public IFluentLambdaItem OnSchedulingFailed(Func<LambdaSchedulingFailedEvent, WorkflowAction> schedulingFailedAction)
        {
            Ensure.NotNull(schedulingFailedAction, nameof(schedulingFailedAction));
            _schedulingFailedAction = schedulingFailedAction;
            return this;
        }

        public IFluentLambdaItem OnStartFailed(Func<LambdaStartFailedEvent, WorkflowAction> startFailedAction)
        {
            Ensure.NotNull(startFailedAction, nameof(startFailedAction));
            _startFailedAction = startFailedAction;
            return this;
        }

        public IFluentLambdaItem When(Func<ILambdaItem, bool> @true)
        {
            Ensure.NotNull(@true, nameof(@true));
            _whenFunc = @true;
            return this;
        }

        public IFluentLambdaItem When(Func<ILambdaItem, bool> @true, Func<ILambdaItem, WorkflowAction> onFalseAction)
        {
            Ensure.NotNull(@true, nameof(@true));
            Ensure.NotNull(onFalseAction, nameof(onFalseAction));
            _whenFunc = @true;
            _onFalseTrigger = onFalseAction;
            return this;
        }

        public WorkflowAction CompletedWorkflowAction(LambdaCompletedEvent @event)
        {
            return _completedAction(@event);
        }

        public WorkflowAction FailedWorkflowAction(LambdaFailedEvent @event)
        {
            return _failedAction(@event);
        }

        public WorkflowAction TimedoutWorkflowAction(LambdaTimedoutEvent @event)
        {
            return _timedoutAction(@event);
        }

        public WorkflowAction SchedulingFailedWorkflowAction(LambdaSchedulingFailedEvent @event)
        {
            return _schedulingFailedAction(@event);
        }

        public WorkflowAction StartFailedWorkflowAction(LambdaStartFailedEvent @event)
        {
            return _startFailedAction(@event);
        }
    }
}