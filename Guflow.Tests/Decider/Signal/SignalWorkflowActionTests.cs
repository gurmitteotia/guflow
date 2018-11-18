// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalWorkflowActionTests
    {
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
        }
        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnSignal("name","input","id","runid");
            var timerFiredEventGraph= _builder.TimerFiredGraph(Identity.Timer("timer1").ScheduleId(), TimeSpan.FromSeconds(2));
            var timerEvent = new TimerFiredEvent(timerFiredEventGraph.First(),timerFiredEventGraph);

            var decisions = timerEvent.Interpret(workflow).Decisions();

            Assert.That(decisions,Is.EqualTo(new []{new SignalWorkflowDecision("name","input","id","runid")}));
        }

        private class WorkflowToReturnSignal : Workflow
        {
            public WorkflowToReturnSignal(string name, string input, string workflowId, string runid)
            {
                ScheduleTimer("timer1").OnFired(e => Signal(name, input).ForWorkflow(workflowId, runid));
            }
        }
    }
}