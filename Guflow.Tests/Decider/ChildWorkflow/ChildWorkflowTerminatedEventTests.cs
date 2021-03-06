﻿// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowTerminatedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        private ChildWorkflowTerminatedEvent _event;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private const string WorkflowRunId = "runid";
            
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder().AddWorkflowRunId(WorkflowRunId);
            var scheduleId = Identity.New(WorkflowName, WorkflowVersion, PositionalName).ScheduleId(WorkflowRunId);
            var eventGraph = _eventGraphBuilder.ChildWorkflowTerminatedEventGraph(scheduleId, "runid", "input").ToArray();
            _builder.AddNewEvents(eventGraph);
            _event = new ChildWorkflowTerminatedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_the_properties_from_history_event_graph()
        {
            Assert.That(_event.Input, Is.EqualTo("input"));
            Assert.That(_event.IsActive, Is.False);
            Assert.That(_event.RunId, Is.EqualTo("runid"));
        }

        [Test]
        public void By_default_fail_the_workflow()
        {
            var decisions = new ChildWorkflow().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]
            {
                new FailWorkflowDecision("CHILD_WORKFLOW_TERMINATED", $"Name={WorkflowName}, Version={WorkflowVersion}, PositionalName={PositionalName}")
            }));
        }
        [Test]
        public void Can_return_a_custom_workflow_action()
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
                    .OnTerminated(_ => CompleteWorkflow(completeResult));
            }
        }
    }
}