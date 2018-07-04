// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaStartedEventTests
    {
        private HistoryEventsBuilder _builder;
        private LambdaStartedEvent _event;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            var eventGraph = _builder.LambdaStartedEventGraph(Identity.Lambda("lambda_name"), "input", "control", TimeSpan.FromSeconds(10));
            _event = new LambdaStartedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_properties_from_failed_history_event()
        {
            Assert.That(_event.Input, Is.EqualTo("input"));
            Assert.That(_event.Timeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(_event.IsActive);
        }

        [Test]
        public void Throws_exception_when_interpreted()
        {
            Assert.Throws<NotSupportedException>(() => _event.Interpret(new EmptyWorkflow()));
        }
    }
}