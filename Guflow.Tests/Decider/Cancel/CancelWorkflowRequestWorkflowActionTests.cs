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
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
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
            const string runId = "runid";
            var workflow = new WorkflowToReturnCancelRequest("id", "other workflow runid");
            var scheduleId = Identity.Timer("timer1").ScheduleId();
            _builder.AddNewEvents(_graphBuilder.TimerFiredGraph(scheduleId, TimeSpan.FromSeconds(2)).ToArray());
            _builder.AddWorkflowRunId(runId);

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new CancelRequestWorkflowDecision("id", "other workflow runid") }));
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