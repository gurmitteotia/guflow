// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkStartedEventTests
    {
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        private ChildWorkflowStartedEvent _completedEvent;
        private HistoryEvent[] _eventGraph;
        private ScheduleId _scheduleId;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _scheduleId = Identity.New(WorkflowName, WorkflowVersion, PositionalName).ScheduleId();
            _eventGraph = _eventGraphBuilder.ChildWorkflowStartedEventGraph(_scheduleId, "runid", "input").ToArray();
            _completedEvent = new ChildWorkflowStartedEvent(_eventGraph.First(), _eventGraph);
        }

        [Test]
        public void Populate_the_properties_from_history_event_graph()
        {
            Assert.That(_completedEvent.Input, Is.EqualTo("input"));
            Assert.That(_completedEvent.IsActive, Is.True);
            Assert.That(_completedEvent.RunId, Is.EqualTo("runid"));
            Assert.That(_completedEvent.WorkflowId, Is.EqualTo(_scheduleId.ToString()));
        }

        [Test]
        public void By_default_it_is_ignored()
        {
            _builder.AddNewEvents(_eventGraph);

            var decisions = new TestWorkflowDefaultAction().Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Can_return_custom_action()
        {
            _builder.AddNewEvents(_eventGraph);
            var w = new TestWorkflow("result");
            var decisions = w.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new CompleteWorkflowDecision("result")}));
        }

        private class TestWorkflowDefaultAction : Workflow
        {
            public TestWorkflowDefaultAction()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName);
            }
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow(string result)
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion, PositionalName)
                    .OnStarted(e => CompleteWorkflow(result));
            }
        }
    }
}