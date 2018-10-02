// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Decider.ChildWorkflow;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowCompletedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private ChildWorkflowCompletedEvent _completedEvent;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private Identity _workflowIdentity;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
             _workflowIdentity = Identity.New(WorkflowName, WorkflowVersion, PositionalName);
            var eventGraph = _eventGraphBuilder.ChildWorkflowCompletedGraph(_workflowIdentity, "runid", "input", "result");
            _completedEvent = new ChildWorkflowCompletedEvent(eventGraph.First() , eventGraph);
        }

        [Test]
        public void Populate_the_properties_from_history_event_graph()
        {
            Assert.That(_completedEvent.Result, Is.EqualTo("result"));
            Assert.That(_completedEvent.Input, Is.EqualTo("input"));
            Assert.That(_completedEvent.IsActive, Is.False);
            Assert.That(_completedEvent.RunId, Is.EqualTo("runid"));
        }

        [Test]
        public void Throws_exception_when_initiated_event_is_not_found()
        {
            var eventGraph = _eventGraphBuilder.ChildWorkflowCompletedGraph(_workflowIdentity, "runid", "input", "result");
            Assert.Throws<IncompleteEventGraphException>(() =>
                new ChildWorkflowCompletedEvent(eventGraph.First(), eventGraph.Take(2)));
        }

        [Test]
        public void By_default_schedule_children()
        {
            var decisions = _completedEvent.Interpret(new ChildWorkflow()).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleTimerDecision(Identity.Timer("TimerName"), TimeSpan.Zero)}));
        }

        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);

                ScheduleTimer("TimerName").AfterChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);
            }
        }
    }
}