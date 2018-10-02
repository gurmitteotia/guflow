// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using Guflow.Decider.ChildWorkflow;
using Guflow.Worker;

namespace Guflow.Decider
{
    internal class ChildWorkflowItem : WorkflowItem, IFluentChildWorkflowItem
    {
        public ChildWorkflowItem(Identity identity, IWorkflow workflow):base(identity, workflow)
        {
            throw new System.NotImplementedException();
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
    }
}