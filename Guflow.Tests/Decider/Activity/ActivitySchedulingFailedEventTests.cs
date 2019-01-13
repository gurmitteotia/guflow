// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ActivitySchedulingFailedEventTests
    {
        private ActivitySchedulingFailedEvent _activitySchedulingFailedEvent;
        private const string ActivityName = "Download";
        private const string ActivityVersion = "1.0";
        private const string PositionalName = "First";
        private const string _cause = "detail";
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private HistoryEvent[] _eventGraph;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _eventGraph = _graphBuilder
                .ActivitySchedulingFailedGraph(Identity.New(ActivityName, ActivityVersion, PositionalName).ScheduleId(),
                    _cause).ToArray();
            _activitySchedulingFailedEvent = new ActivitySchedulingFailedEvent(_eventGraph.First());
            _builder.AddNewEvents(_eventGraph);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_activitySchedulingFailedEvent.Cause, Is.EqualTo(_cause));
            Assert.That(_activitySchedulingFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_should_fail_workflow()
        {
            var workflow = new SingleActivityWorkflow();

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new[]{new FailWorkflowDecision("ACTIVITY_SCHEDULING_FAILED",_cause)}));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var workflow = new WorkflowWithCustomAction("result");
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new[]{new CompleteWorkflowDecision("result")}));
        }

        private class SingleActivityWorkflow : Workflow
        {
            public SingleActivityWorkflow()
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(string result)
            {
                ScheduleActivity(ActivityName, ActivityVersion, PositionalName).OnSchedulingFailed(e => CompleteWorkflow(result));
            }
        }
    }
}