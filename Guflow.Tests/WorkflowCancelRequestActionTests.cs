using System;
using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowCancelRequestActionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.That(WorkflowAction.CancelRequest("wid","rid").Equals(WorkflowAction.CancelRequest("wid","rid")));

            Assert.False(WorkflowAction.CancelRequest("wid", "rid").Equals(WorkflowAction.CancelRequest("wid", "rid1")));
            Assert.False(WorkflowAction.CancelRequest("wid", "rid").Equals(WorkflowAction.CancelRequest("wid1", "rid")));
        }

        [Test]
        public void Returns_cancel_request_workflow_decision()
        {
            var workflowDecisions = WorkflowAction.CancelRequest("wid", "rid").GetDecisions();

            Assert.That(workflowDecisions,Is.EqualTo(new[]{new CancelRequestWorkflowDecision("wid","rid")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnCancelRequest("id", "runid");
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer1"), TimeSpan.FromSeconds(2));
            var timerEvent = new TimerFiredEvent(timerFiredEventGraph.First(), timerFiredEventGraph);

            var workflowAction = timerEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.CancelRequest("id", "runid")));
        }

        private class WorkflowToReturnCancelRequest : Workflow
        {
            public WorkflowToReturnCancelRequest(string workflowId, string runid)
            {
                ScheduleTimer("timer1").OnFired(e => CancelRequest(workflowId, runid));
            }
        }
    }
}