// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaStartFailedEventTests
    {
        private EventGraphBuilder _builder;
        private LambdaStartFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var eventGraph = _builder.LambdaStartFailedEventGraph(Identity.Lambda("lambda_name").ScheduleId(), "input", "reason", "message", TimeSpan.FromSeconds(10));
            _event = new LambdaStartFailedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_properties_from_failed_history_event()
        {
            Assert.That(_event.Cause, Is.EqualTo("reason"));
            Assert.That(_event.Message, Is.EqualTo("message"));
            Assert.That(_event.Timeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.IsFalse(_event.IsActive);
        }

        [Test]
        public void By_default_fails_the_workflow()
        {
            var decision = _event.Interpret(new WorkflowWithLambda()).Decisions(Mock.Of<IWorkflow>());

            Assert.That(decision, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "message") }));
        }

        [Test]
        public void Can_return_custom_action()
        {
            var customAction = WorkflowAction.CompleteWorkflow("result");
            var workflowAction = _event.Interpret(new WorkflowWithCustomAction(customAction));

            Assert.That(workflowAction, Is.EqualTo(customAction));
        }

        private class WorkflowWithLambda : Workflow
        {
            public WorkflowWithLambda()
            {
                ScheduleLambda("lambda_name");

                ScheduleTimer("timer_name").AfterLambda("lambda_name");
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleLambda("lambda_name").OnStartFailed(e => workflowAction);

                ScheduleTimer("timer_name").AfterLambda("lambda_name");
            }
        }
    }
}