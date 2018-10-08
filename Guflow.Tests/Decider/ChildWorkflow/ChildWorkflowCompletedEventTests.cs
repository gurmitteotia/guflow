﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowCompletedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private ChildWorkflowCompletedEvent _event;

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
            _event = new ChildWorkflowCompletedEvent(eventGraph.First() , eventGraph);
        }

        [Test]
        public void Populate_the_properties_from_history_event_graph()
        {
            Assert.That(_event.Result, Is.EqualTo("result"));
            Assert.That(_event.Input, Is.EqualTo("input"));
            Assert.That(_event.IsActive, Is.False);
            Assert.That(_event.RunId, Is.EqualTo("runid"));
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
            var decisions = _event.Interpret(new ChildWorkflow()).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleTimerDecision(Identity.Timer("TimerName"), TimeSpan.Zero)}));
        }

        [Test]
        public void Throws_exception_when_child_workflow_item_is_not_found_in_workflow()
        {
           Assert.Throws<IncompatibleWorkflowException>(()=> _event.Interpret(new EmptyWorkflow()).Decisions());
        }

        [Test]
        public void Can_return_a_custom_workflow_action()
        {
            var decisions = _event.Interpret(new ChildWorkflowWithCustomAction("result")).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result")}));
        }

        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);

                ScheduleTimer("TimerName").AfterChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);
            }
        }

        private class ChildWorkflowWithCustomAction : Workflow
        {
            public ChildWorkflowWithCustomAction(string completeResult)
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName)
                    .OnCompletion(_ => CompleteWorkflow(completeResult));
            }
        }
    }
}