// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerCancellationFailedEventTests
    {
        private TimerCancellationFailedEvent _timerCancellationFailedEvent;
        private const string TimerName = "timer";
        private const string Cause = "cause";
        private const string ActivityName = "activity";
        private const string ActivityVersion = "1.0";
        private const string LambdaName = "Lambda";
        private EventGraphBuilder _builder;
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.Timer(TimerName), Cause);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_timerCancellationFailedEvent.Cause,Is.EqualTo(Cause));
            Assert.IsFalse(_timerCancellationFailedEvent.IsActive);
        }

        [Test]
        public void Throws_exception_when_timer_is_not_found()
        {
            Assert.Throws<IncompatibleWorkflowException>(() => _timerCancellationFailedEvent.Interpret(new EmptyWorkflow()));
        }

        [Test]
        public void By_default_return_workflow_failed_action()
        {
            var decisions = _timerCancellationFailedEvent.Interpret(new TestWorkflow()).Decisions();
            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("TIMER_CANCELLATION_FAILED",Cause)}));
        }

        [Test]
        public void Fail_workflow_for_activity_reschedule_timer()
        {
            var workflow = new WorkflowWithActivity();
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.New(ActivityName, ActivityVersion), Cause);
            var decisions = timerCancellationFailedEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("RESCHEDULE_TIMER_CANCELLATION_FAILED", Cause)}));
        }

        [Test]
        public void Fail_workflow_for_lambda_reschedule_timer()
        {
            var workflow = new WorkflowWithLambda();
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.Lambda(LambdaName), Cause);
            var decisions = timerCancellationFailedEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_CANCELLATION_FAILED", Cause) }));
        }

        [Test]
        public void Can_return_custom_workflow_action_from_workflow()
        {
            var customAction = new Mock<WorkflowAction>().Object;
            var workflowAction = _timerCancellationFailedEvent.Interpret(new WorkflowWithCustomAction(customAction));

            Assert.That(workflowAction, Is.EqualTo(customAction));
        }
        
        private TimerCancellationFailedEvent CreateTimerCancellationFailedEvent(Identity identity, string cause)
        {
            var timerCancellationFailedEventGraph = _builder.TimerCancellationFailedGraph(identity, Cause);
            return new TimerCancellationFailedEvent(timerCancellationFailedEventGraph.First());
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleTimer(TimerName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(TimerName).OnCancellationFailed(c => workflowAction);
            }
        }

        private class WorkflowWithActivity : Workflow
        {
            public WorkflowWithActivity()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
            }
        }

        private class WorkflowWithLambda : Workflow
        {
            public WorkflowWithLambda()
            {
                ScheduleLambda(LambdaName);
            }
        }
    }
}