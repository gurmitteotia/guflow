using System;
using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class SignalWorkflowActionTests
    {
        [Test]
        public void Signal_retuns_signal_workflow_action()
        {
            var signalAction = new Signal("name", "input");

            var workflowAction =signalAction.SendTo("id", "runid");

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Signal("name","input","id","runid")));
        }

        [Test]
        public void Invalid_arguments_tests()
        {
            var signalAction = new Signal("name", "input");
            Assert.Throws<ArgumentException>(() => signalAction.SendTo(null, "runid"));
            Assert.Throws<ArgumentNullException>(() => signalAction.ReplyTo(null));
        }

        [Test]
        public void Returns_signal_workflow_decision()
        {
            var signalAction = WorkflowAction.Signal("name", "input", "id", "runid");

            Assert.That(signalAction.GetDecisions(),Is.EquivalentTo(new[]{new SignalWorkflowDecision("name","input","id","runid")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnSignal("name","input","id","runid");
            var timerFiredEventGraph= HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer1"), TimeSpan.FromSeconds(2));
            var timerEvent = new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);

            var workflowAction = timerEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.Signal("name","input","id","runid")));
        }

        private class WorkflowToReturnSignal : Workflow
        {
            public WorkflowToReturnSignal(string name, string input, string workflowId, string runid)
            {
                ScheduleTimer("timer1").OnFired(e => Signal(name, input).SendTo(workflowId, runid));
            }
        }
    }
}