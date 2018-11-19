﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RecordMarkerWorkflowActionTests
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
        public void Returns_record_marker_workflow_decision()
        {
            var recordMarkerDecision = WorkflowAction.RecordMarker("name", "detail");

            var workflowDecisions = recordMarkerDecision.Decisions();

            Assert.That(workflowDecisions,Is.EquivalentTo(new []{ new RecordMarkerWorkflowDecision("name","detail")}));
        }

        [Test]
        public void Can_be_returned_as_custom_action_workflow()
        {
            var timerFiredEventGraph = _graphBuilder.TimerFiredGraph(Identity.Timer("timer1").ScheduleId(), TimeSpan.FromSeconds(2)).ToArray();
            _builder.AddNewEvents(timerFiredEventGraph);

            var decisions = new WorkflowToReturnRecordMarker("markerName", "details").Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new []{new RecordMarkerWorkflowDecision("markerName","details")}));
        }

        private class WorkflowToReturnRecordMarker : Workflow
        {
            public WorkflowToReturnRecordMarker(string markerName, string details)
            {
                ScheduleTimer("timer1").OnFired(e => RecordMarker(markerName, details));
            }
        }
    }
}