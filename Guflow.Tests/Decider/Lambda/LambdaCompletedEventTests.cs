//Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaCompletedEventTests
    {
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private LambdaCompletedEvent _event;
        private IEnumerable<HistoryEvent> _eventGraph;
        private const string LambdaName = "lambda_name";
        private const string  PositionalName = "pos_name";
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _eventGraph = _graphBuilder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName, PositionalName).ScheduleId(), "input", "result", TimeSpan.FromSeconds(10));
            _event = new LambdaCompletedEvent(_eventGraph.First(), _eventGraph);

        }
        [Test]
        public void Properties_are_populated_from_history_event()
        {
            Assert.That(_event.Result, Is.EqualTo("result"));
            Assert.That(_event.Input, Is.EqualTo("input"));
            Assert.IsFalse(_event.IsActive);
        }

        [Test]
        public void Throws_exception_when_lambda_scheduled_event_not_found()
        {
            var eventGraph = _graphBuilder.LambdaCompletedEventGraph(Identity.Lambda(LambdaName).ScheduleId(), "input", "result", TimeSpan.FromSeconds(10));
            Assert.Throws<IncompleteEventGraphException>(()=>_event = new LambdaCompletedEvent(eventGraph.First(), Enumerable.Empty<HistoryEvent>()));
        }

        [Test]
        public void Schedule_children()
        {
            const string runId = "runid";
            _builder.AddWorkflowRunId(runId);
            _builder.AddNewEvents(_eventGraph.ToArray());
            var decisions = new WorkflowWithLambda().Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] {ScheduleTimerDecision.WorkflowItem(Identity.Timer("timer_name").ScheduleId(), TimeSpan.Zero) }));
        }

        [Test]
        public void Can_schedule_custom_action()
        {
            var decisions = _event.Interpret(new WorkflowWithCustomAction(WorkflowAction.CompleteWorkflow("result"))).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result")}));
        }

        private class WorkflowWithLambda : Workflow
        {
            public WorkflowWithLambda()
            {
                ScheduleLambda(LambdaName, PositionalName);

                ScheduleTimer("timer_name").AfterLambda(LambdaName, PositionalName);
            }
        }

        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction action)
            {
                ScheduleLambda(LambdaName, PositionalName).OnCompletion(e=>action);

                ScheduleTimer("timer_name").AfterLambda(LambdaName, PositionalName);
            }
        }
    }
}