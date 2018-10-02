// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class WorkflowActionItem : WorkflowItem, IFluentWorkflowActionItem
    {
        private readonly Func<IWorkflowItem,WorkflowAction> _workflowActionFunc;
        public WorkflowActionItem(Func<IWorkflowItem, WorkflowAction> workflowActionFunc, IWorkflow workflow)
            : base(RandomIdentity(), workflow)
        {
            _workflowActionFunc = workflowActionFunc;
        }
        public override WorkflowItemEvent LastEvent(bool includeRescheduleTimerEvents = false) => null;

        public override IEnumerable<WorkflowDecision> ScheduleDecisions()
        {
            return ScheduleDecisionsByIgnoringWhen();
        }

        public override IEnumerable<WorkflowDecision> ScheduleDecisionsByIgnoringWhen()
        {
            return _workflowActionFunc(this).Decisions();
        }

        public override IEnumerable<WorkflowDecision> RescheduleDecisions(TimeSpan timeout)
        {
            return ScheduleDecisions();
        }

        public override IEnumerable<WorkflowDecision> CancelDecisions()
        {
            return Enumerable.Empty<WorkflowDecision>();
        }
        private static Identity RandomIdentity()
        {
            return Identity.New(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public IFluentWorkflowActionItem AfterTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            AddParent(Identity.Timer(name));
            return this;
        }

        public IFluentWorkflowActionItem AfterActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));

            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentWorkflowActionItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }

        public IFluentWorkflowActionItem AfterLambda(string name, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));

            AddParent(Identity.Lambda(name, positionalName));
            return this;
        }

        public IFluentWorkflowActionItem AfterChildWorkflow(string name, string version, string positionalName = "")
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<WorkflowItemEvent> AllEvents(bool includeRescheduleTimerEvents = false)
                                            => Enumerable.Empty<WorkflowItemEvent>();
    }
}