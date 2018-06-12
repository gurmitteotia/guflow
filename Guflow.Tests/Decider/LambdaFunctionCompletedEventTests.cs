//Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaFunctionCompletedEventTests
    {
        private HistoryEventsBuilder _builder;
        private LamdbaFunctionCompletedEvent _event;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }
        [Test]
        public void Properties_are_populated_from_history_event()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph("Id", "lambda_name", "input", "result", "", TimeSpan.FromSeconds(10));
            _event = new LamdbaFunctionCompletedEvent(eventGraph.First(), eventGraph);

            Assert.That(_event.Name, Is.EqualTo("lambda_name"));
            Assert.That(_event.Result, Is.EqualTo("result"));
            Assert.That(_event.Input, Is.EqualTo("input"));
        }

        [Test]
        public void Throws_exception_when_lambda_scheduled_event_not_found()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph("Id", "lambda_name", "input", "result", "", TimeSpan.FromSeconds(10));
            Assert.Throws<IncompleteEventGraphException>(()=>_event = new LamdbaFunctionCompletedEvent(eventGraph.First(), Enumerable.Empty<HistoryEvent>()));
        }

        //[Test]
        //public void Schedule_children()
        //{
        //    var eventGraph = _builder.LambdaCompletedEventGraph("Id", "lambda_name", "input", "result", "", TimeSpan.FromSeconds(10));
        //    _event = new LamdbaFunctionCompletedEvent(eventGraph.First(), eventGraph);
        //    var decisions = _event.Interpret(new WorkflowWithLambda()).Decisions();

        //    Assert.That(decisions, Is.EqualTo(new[]{new ScheduleTimerDecision(Identity.Timer("timer"),TimeSpan.Zero)}));
        //}

        //private class WorkflowWithLambda : Workflow
        //{
        //    public WorkflowWithLambda()
        //    {
        //        ScheduleLambda("lambda_name");

        //        ScheduleTimer("timer_name").AfterLambda("lambda_name");
        //    }
        //}
    }
}