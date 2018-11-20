// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaSchedulingFailedEventTests
    {
        private EventGraphBuilder _builder;
        private LambdaSchedulingFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            var eventGraph = _builder.LambdaSchedulingFailedEventGraph(Identity.Lambda("lambda_name").ScheduleId(), "reason");
            _event = new LambdaSchedulingFailedEvent(eventGraph);
        }

        [Test]
        public void Populate_properties_from_failed_history_event()
        {
            Assert.That(_event.Cause, Is.EqualTo("reason"));
            Assert.IsFalse(_event.IsActive);
        }


        [Test]
        public void By_default_fails_the_workflow()
        {
            var decision = _event.Interpret(new WorkflowWithLambda()).Decisions();

            Assert.That(decision, Is.EqualTo(new[] { new FailWorkflowDecision("LAMBDA_FUNCTION_SCHEDULING_FAILED", "reason") }));
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
                ScheduleLambda("lambda_name").OnSchedulingFailed(e => workflowAction);

                ScheduleTimer("timer_name").AfterLambda("lambda_name");
            }
        }
    }
}