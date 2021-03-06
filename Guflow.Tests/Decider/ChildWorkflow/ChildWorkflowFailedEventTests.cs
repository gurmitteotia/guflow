﻿// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using System.Runtime.InteropServices;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowFailedEventTests
    {
        private ChildWorkflowFailedEvent _event;
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private const string ParentWorkflowRunId = "Pid";

        private ScheduleId _workflowIdentity;

        [SetUp]
        public void Setup()
        {
            _workflowIdentity = Identity.New(WorkflowName, WorkflowVersion, PositionalName)
                .ScheduleId(ParentWorkflowRunId);
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder().AddWorkflowRunId(ParentWorkflowRunId);

            var eventGraph = _eventGraphBuilder.ChildWorkflowFailedEventGraph(_workflowIdentity, "rid", "input", "reason",
                    "details").ToArray();
            _builder.AddNewEvents(eventGraph);
            _event =new ChildWorkflowFailedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_properties_from_event_graph()
        {
            Assert.That(_event.Reason, Is.EqualTo("reason"));
            Assert.That(_event.Details, Is.EqualTo("details"));
            Assert.That(_event.RunId, Is.EqualTo("rid"));
            Assert.That(_event.Input, Is.EqualTo("input"));
        }

        [Test]
        public void By_default_fail_workflow()
        {
            var decisions = new ChildWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]{new FailWorkflowDecision("reason", "details")}));
        }

        [Test]
        public void Can_return_custom_action()
        {
            var decisions = new ChildWorkflowWithCustomAction("result").Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);
            }
        }

        private class ChildWorkflowWithCustomAction : Workflow
        {
            public ChildWorkflowWithCustomAction(string completeResult)
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName)
                    .OnFailure(_ => CompleteWorkflow(completeResult));
            }
        }
    }
}