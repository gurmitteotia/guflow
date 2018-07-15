// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{

    [TestFixture]
    public class LambdaFailedEventTests
    {
        private EventGraphBuilder _builder;
        private LambdaFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var eventGraph = _builder.LambdaFailedEventGraph(Identity.Lambda("lambda_name"), "input", "reason", "details");
            _event = new LambdaFailedEvent(eventGraph.First(), eventGraph);
        }

        [Test]
        public void Populate_properties_from_failed_history_event()
        {
            Assert.That(_event.Input, Is.EqualTo("input"));
            Assert.That(_event.Reason, Is.EqualTo("reason"));
            Assert.That(_event.Details, Is.EqualTo("details"));
            Assert.IsFalse(_event.IsActive);
        }

        [Test]
        public void By_default_fails_the_workflow()
        {
            var decision = _event.Interpret(new WorkflowWithLambda()).Decisions();

            Assert.That(decision, Is.EqualTo(new[]{new FailWorkflowDecision("reason", "details")}));
        }

        [Test]
        public void Can_return_custom_action()
        {
            var customAction = WorkflowAction.CompleteWorkflow("result");
            var workflowAction = _event.Interpret(new WorkflowWithCustomAction(customAction));

            Assert.That(workflowAction, Is.EqualTo(customAction));
        }

        [Test]
        public void Throws_exception_when_lamdba_is_not_found_for_failed_event()
        {
            var eventGraph = _builder.LambdaFailedEventGraph(Identity.Lambda("differnt_name"), "input", "reason", "details");
            var @event = new LambdaFailedEvent(eventGraph.First(), eventGraph);
            Assert.Throws<IncompatibleWorkflowException>(()=> @event.Interpret(new WorkflowWithLambda()));
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
                ScheduleLambda("lambda_name").OnFailure(e=>workflowAction);

                ScheduleTimer("timer_name").AfterLambda("lambda_name");
            }
        }
    }
}