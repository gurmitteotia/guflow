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

        public override WorkflowItemEvent LastEvent => null;

        public override IEnumerable<WorkflowDecision> GetScheduleDecisions()
        {
            return _workflowActionFunc(this).Decisions();
        }

        public override IEnumerable<WorkflowDecision> GetRescheduleDecisions(TimeSpan timeout)
        {
            return GetScheduleDecisions();
        }

        public override WorkflowDecision GetCancelDecision()
        {
            return WorkflowDecision.Empty;
        }
        private static Identity RandomIdentity()
        {
            return Identity.New(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        public IFluentWorkflowActionItem AfterTimer(string name)
        {
            AddParent(Identity.Timer(name));
            return this;
        }

        public IFluentWorkflowActionItem AfterActivity(string name, string version, string positionalName = "")
        {
            AddParent(Identity.New(name, version, positionalName));
            return this;
        }

        public IFluentWorkflowActionItem AfterActivity<TActivity>(string positionalName = "") where TActivity : Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }
        public override IEnumerable<WorkflowItemEvent> AllEvents => Enumerable.Empty<WorkflowItemEvent>();
    }
}