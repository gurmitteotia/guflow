// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkStartedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private ChildWorkflowStartedEvent _completedEvent;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            var scheduleId = Identity.New(WorkflowName, WorkflowVersion, PositionalName).ScheduleId();
            var eventGraph = _eventGraphBuilder.ChildWorkflowStartedEventGraph(scheduleId, "runid", "input");
            _completedEvent = new ChildWorkflowStartedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_the_properties_from_history_event_graph()
        {
            Assert.That(_completedEvent.Input, Is.EqualTo("input"));
            Assert.That(_completedEvent.IsActive, Is.True);
            Assert.That(_completedEvent.RunId, Is.EqualTo("runid"));
        }

        [Test]
        public void By_default_it_is_ignored()
        {
            var decisions = _completedEvent.Interpret(new EmptyWorkflow()).Decisions();

            Assert.That(decisions, Is.Empty);
        }
    }
}