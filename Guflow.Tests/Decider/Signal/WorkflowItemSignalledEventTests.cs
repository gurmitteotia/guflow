// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowItemSignalledEventTests
    {
        private EventGraphBuilder _graphBuilder;
        private WorkflowItemSignalledEvent _event;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _event = new WorkflowItemSignalledEvent(_graphBuilder.SignalResumedEvent(ScheduleId.Raw("id"), 10, "e1"));
        }

        [Test]
        public void Populate_properties()
        {
            Assert.That(_event.TriggerEventId, Is.EqualTo(_event.TriggerEventId));
            Assert.That(_event.IsActive, Is.False);
        }

        [Test]
        public void Throws_exception_when_interpreted()
        {
            Assert.Throws<NotSupportedException>(()=>_event.Interpret(new EmptyWorkflow()));
        }
    }
}