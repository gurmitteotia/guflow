// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ExternalWorkflowCancellationRequestedEventTests
    {
        private ExternalWorkflowCancellationRequestedEvent _event;
        private EventGraphBuilder _eventGraphBuilder;

        private const string WorkflowName = "workflow";
        private const string WorkflowVersion = "1.0";
        private const string PositionalName = "Pos";
        private Identity _workflowIdentity;

        [SetUp]
        public void Setup()
        {
            _workflowIdentity = Identity.New(WorkflowName, WorkflowVersion, PositionalName);
            _eventGraphBuilder = new EventGraphBuilder();
            var eventGraph =
                _eventGraphBuilder.ChildWorkflowCancellationRequestedEventGraph(_workflowIdentity, "rid", "input").ToArray();
            _event = new ExternalWorkflowCancellationRequestedEvent(eventGraph.First());
        }

        [Test]
        public void Active()
        {
            Assert.That(_event.IsActive);
        }
    }
}