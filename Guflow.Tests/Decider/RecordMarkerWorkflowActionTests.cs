using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RecordMarkerWorkflowActionTests
    {
        [Test]
        public void Equality_tests()
        {
            Assert.That(WorkflowAction.RecordMarker("name","detail").Equals(WorkflowAction.RecordMarker("name","detail")));

            Assert.False(WorkflowAction.RecordMarker("name", "detail").Equals(WorkflowAction.RecordMarker("name", "detail1")));
            Assert.False(WorkflowAction.RecordMarker("name", "detail").Equals(WorkflowAction.RecordMarker("name1", "detail")));
        }

        [Test]
        public void Returns_record_marker_workflow_decision()
        {
            var recordMarkerDecision = WorkflowAction.RecordMarker("name", "detail");

            var workflowDecisions = recordMarkerDecision.GetDecisions();

            Assert.That(workflowDecisions,Is.EquivalentTo(new []{ new RecordMarkerWorkflowDecision("name","detail")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_workflow()
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer1"), TimeSpan.FromSeconds(2));
            var timerFiredEvent = new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);

            var workflowAction = timerFiredEvent.Interpret(new WorkflowToReturnRecordMarker("markerName", "details"));

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.RecordMarker("markerName","details")));
        }

        private class WorkflowToReturnRecordMarker : Workflow
        {
            public WorkflowToReturnRecordMarker(string markerName, string details)
            {
                ScheduleTimer("timer1").OnFired(e => RecordMarker(markerName, details));
            }
        }
    }
}