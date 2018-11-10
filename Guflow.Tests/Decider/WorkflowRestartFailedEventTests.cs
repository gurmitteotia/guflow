// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowRestartFailedEventTests
    {
        private WorkflowRestartFailedEvent _failedEvent;
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _failedEvent = new WorkflowRestartFailedEvent(_builder.WorkflowRestartFailedEventGraph("cause"));
        }

        [Test]
        public void Populate_properties_from_event_graph()
        {
            Assert.That(_failedEvent.Cause, Is.EqualTo("cause"));
        }

        [Test]
        public void By_default_fails_workflow()
        {
            var decisions = _failedEvent.Interpret(new EmptyWorkflow()).Decisions();

            Assert.That(decisions, Is.EqualTo(new[]{new FailWorkflowDecision("FAILED_TO_RESTART_WORKFLOW", "cause")}));
        }

        [Test]
        public void Can_return_custom_action()
        {
            var decisions = _failedEvent.Interpret(new WorkflowWithCompleteAction("result")).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        [WorkflowDescription("1.0")]
        private class EmptyWorkflow : Workflow
        {
        }

        [WorkflowDescription("1.0")]
        private class WorkflowWithCompleteAction : Workflow
        {
            private readonly string _result;

            public WorkflowWithCompleteAction(string result)
            {
                _result = result;
            }

            [WorkflowEvent(EventName.RestartFailed)]
            public WorkflowAction OnError() => CompleteWorkflow(_result);
        }
    }
}