// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class LambdaItem : WorkflowItem, IFluentLambdaItem, ILambdaItem
    {
        private Func<ILambdaItem, object> _input;
        private Func<ILambdaItem, TimeSpan?> _timeout;
        private TimerItem _rescheduleTimer;
        public LambdaItem(Identity identity, IWorkflow workflow) : base(identity, workflow)
        {
            _input = (item) => WorkflowHistoryEvents.WorkflowStartedEvent().Input;
            _timeout = item => null;
            _rescheduleTimer = TimerItem.Reschedule(this, identity, workflow);
        }

        public string PositionalName => Identity.PositionalName;

        public override WorkflowItemEvent LastEvent { get; }
        public override IEnumerable<WorkflowItemEvent> AllEvents { get; }
        public override IEnumerable<WorkflowDecision> GetScheduleDecisions()
        {
            return new []{new ScheduleLambdaDecision(Identity, _input(this), _timeout(this))};
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
    }
}