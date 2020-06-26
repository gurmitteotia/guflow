// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowCompletedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        private ChildWorkflowCompletedEvent _event;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private const string ParentWorkflowRunId = "Pid";
        private ScheduleId _workflowIdentity;
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddWorkflowRunId(ParentWorkflowRunId);

             _workflowIdentity = Identity.New(WorkflowName, WorkflowVersion, PositionalName).ScheduleId(ParentWorkflowRunId);
            var eventGraph = _eventGraphBuilder.ChildWorkflowCompletedGraph(_workflowIdentity, "runid", "input", "result").ToArray();
            _builder.AddNewEvents(eventGraph);
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
            var decisions = new ChildWorkflow().Decisions(_builder.Result()).ToArray();

            var scheduleId = Identity.Timer("TimerName").ScheduleId();
            Assert.That(decisions.Length, Is.EqualTo(1));
            decisions[0].AssertWorkflowItemTimer(scheduleId, TimeSpan.Zero);
        }

        [Test]
        public void Throws_exception_when_child_workflow_item_is_not_found_in_workflow()
        {
           Assert.Throws<IncompatibleWorkflowException>(()=> _event.Interpret(new EmptyWorkflow()).Decisions(Mock.Of<IWorkflow>()));
        }

        [Test]
        public void Can_return_a_custom_workflow_action()
        {
            var decisions = new ChildWorkflowWithCustomAction("result").Decisions(_builder.Result());

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