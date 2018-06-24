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

        private readonly TimerItem _rescheduleTimer;
        public LambdaItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
            _input = (item) => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _timeout = item => null;
            _rescheduleTimer = TimerItem.Reschedule(this, identity, workflow);
            _completedAction = _ => WorkflowAction.ContinueWorkflow(this);
            _failedAction = @event => WorkflowAction.FailWorkflow(@event.Reason, @event.Details);
            _timedoutAction = @event => WorkflowAction.FailWorkflow("LAMBDA_FUNCTION_TIMED_OUT", @event.TimedoutType);
            _schedulingFailedAction = @event => WorkflowAction.FailWorkflow("LAMBDA_FUNCTION_SCHEDULING_FAILED", @event.Cause);
            _startFailedAction = @event => WorkflowAction.FailWorkflow(@event.Cause, @event.Message);
        }

        public string PositionalName => Identity.PositionalName;

        public override WorkflowItemEvent LastEvent
        {
            get
            {
                var lambdaEvent = WorkflowHistoryEvents.LastLambdaEvent(this);
                var timerEvent = WorkflowHistoryEvents.LastTimerEvent(_rescheduleTimer);
                if (lambdaEvent > timerEvent) return lambdaEvent;
                return timerEvent;
            }
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get
            {
                var lambdaEvents = WorkflowHistoryEvents.AllLambdaEvents(this);
                var timerEvents = WorkflowHistoryEvents.AllTimerEvents(_rescheduleTimer);
                return lambdaEvents.Concat(timerEvents).OrderByDescending(i => i, WorkflowEvent.IdComparer);
            }
        }
        public override IEnumerable<WorkflowDecision> GetScheduleDecisions()
        {
            return new[] { new ScheduleLambdaDecision(Identity, _input(this), _timeout(this)) };
        }

        public override IEnumerable<WorkflowDecision> GetRescheduleDecisions(TimeSpan timeout)
        {
            _rescheduleTimer.FireAfter(timeout);
            return _rescheduleTimer.GetScheduleDecisions();
        }

        public override WorkflowDecision GetCancelDecision()
        {
            return WorkflowDecision.Empty;
        }

        public IFluentActivityItem AfterTimer(string name)
        {
            throw new NotImplementedException();
        }

        public IFluentActivityItem AfterActivity(string name, string version, string positionalName = "")
        {
            throw new NotImplementedException();
        }

        public IFluentActivityItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            throw new NotImplementedException();
        }

        public IFluentActivityItem AfterLambda(string name, string positionalName = "")
        {
            throw new NotImplementedException();
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