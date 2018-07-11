// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class CancelWorkflowRequestWorkflowActionTests
    {
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
        }


        [Test]
        public void Returns_cancel_request_workflow_decision()
        {
            var workflowDecisions = WorkflowAction.CancelWorkflowRequest("wid", "rid").Decisions();

            Assert.That(workflowDecisions,Is.EqualTo(new[]{new CancelRequestWorkflowDecision("wid","rid")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelRequest("id", "runid");
            var timerFiredEventGraph = _builder.TimerFiredGraph(Identity.Timer("timer1"), TimeSpan.FromSeconds(2));
            var timerEvent = new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);

            var decisions = timerEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new CancelRequestWorkflowDecision("id", "runid")}));
        }

        private class WorkflowToReturnCancelRequest : Workflow
        {
            public WorkflowToReturnCancelRequest(string workflowId, string runid)
            {
                ScheduleTimer("timer1").OnFired(e => CancelRequest.ForWorkflow(workflowId, runid));
            }
        }
    }
}