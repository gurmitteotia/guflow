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

        public override WorkflowItemEvent LastEvent
        {
            get { return WorkflowItemEvent.NotFound; }
        }

        public override WorkflowDecision GetScheduleDecision()
        {
            return WorkflowDecision.Empty;
        }

        public override WorkflowDecision GetRescheduleDecision(TimeSpan afterTimeout)
        {
            return WorkflowDecision.Empty;
        }

        public override WorkflowDecision GetCancelDecision()
        {
            return WorkflowDecision.Empty;
        }

        public override IEnumerable<WorkflowDecision> GetContinuedDecisions()
        {
            return _workflowActionFunc(this).GetDecisions();
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
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return AfterActivity(description.Name, description.Version, positionalName);
        }
        public override IEnumerable<WorkflowItemEvent> AllEvents
        {
            get { return Enumerable.Empty<WorkflowItemEvent>(); }
        }
    }
}