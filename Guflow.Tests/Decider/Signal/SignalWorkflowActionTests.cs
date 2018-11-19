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
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
        }
        [Test]
        public void Can_be_returned_as_custom_action_from_workflow()
        {
            var workflow = new WorkflowToReturnSignal("name","input","id","runid");
            var timerFiredEventGraph= _graphBuilder.TimerFiredGraph(Identity.Timer("timer1").ScheduleId(), TimeSpan.FromSeconds(2));
            _builder.AddNewEvents(timerFiredEventGraph.ToArray());

            var decisions = workflow.Decisions(_builder.Result());

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