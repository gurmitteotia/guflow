// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerStartFailedEventTests
    {
        private TimerStartFailedEvent _timerStartFailedEvent;
        private const string TimerFailureCause = "something fancy";
        private const string TimerName = "timername";
        private const string ActivityName = "activity";
        private const string ActivityVersion = "1.0";
        private const string LambdaName = "Lambda";
        private const string WorkflowName = "Workflow";
        private const string WorkflowVersion = "1.0";

        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();

            _timerStartFailedEvent = CreateTimerStartFailedEvent(Identity.Timer(TimerName), TimerFailureCause);
        }

        [Test]
        public void Should_populate_the_properties_from_history_event_attributes()
        {
            Assert.That(_timerStartFailedEvent.Cause,Is.EqualTo(TimerFailureCause));
            Assert.That(_timerStartFailedEvent.IsActive,Is.False);
        }

        [Test]
        public void By_default_returns_fail_workflow_decision()
        {
            var workflow = new WorkflowWithTimer();

            var decisions = _timerStartFailedEvent.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("TIMER_START_FAILED", TimerFailureCause)}));
        }

        [Test]
        public void Can_return_the_custom_action()
        {
            var workflowAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(workflowAction.Object);

            var action = _timerStartFailedEvent.Interpret(workflow);

            Assert.That(action,Is.EqualTo(workflowAction.Object));
        }

        [Test]
        public void Fail_workflow_for_activity_reshedule_timer()
        {
            var workflow = new WorkflowWithActivity();
            var rescheduleTimerStartFailed = CreateTimerStartFailedEvent(Identity.New(ActivityName,ActivityVersion),TimerFailureCause);

            var decisions = rescheduleTimerStartFailed.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("RESCHEDULE_TIMER_START_FAILED", TimerFailureCause)}));
        }

        [Test]
        public void Fail_workflow_for_lambda_reshedule_timer()
        {
            var workflow = new WorkflowWithLambda();
            var rescheduleTimerStartFailed = CreateTimerStartFailedEvent(Identity.Lambda(LambdaName), TimerFailureCause);

            var decisions = rescheduleTimerStartFailed.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_START_FAILED", TimerFailureCause) }));
        }

        [Test]
        public void Fail_workflow_for_child_workflow_reshedule_timer()
        {
            var workflow = new WorkflowWithChildWorkflow();
            var rescheduleTimerStartFailed = CreateTimerStartFailedEvent(Identity.New(WorkflowName, WorkflowVersion), TimerFailureCause);

            var decisions = rescheduleTimerStartFailed.Interpret(workflow).Decisions();

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_START_FAILED", TimerFailureCause) }));
        }
        private TimerStartFailedEvent CreateTimerStartFailedEvent(Identity timerIdentity, string cause)
        {
            var timerFailedEventGraph = _builder.TimerStartFailedGraph(timerIdentity, cause);
            return new TimerStartFailedEvent(timerFailedEventGraph.First());
        }

        private class WorkflowWithActivity : Workflow
        {
            public WorkflowWithActivity()
            {
                ScheduleActivity(ActivityName, ActivityVersion);
            }
        }
        private class WorkflowWithTimer : Workflow
        {
            public WorkflowWithTimer()
            {
                ScheduleTimer(TimerName);
            }
        }

        private class WorkflowWithLambda : Workflow
        {
            public WorkflowWithLambda()
            {
                ScheduleLambda(LambdaName);
            }
        }

        private class WorkflowWithChildWorkflow : Workflow
        {
            public WorkflowWithChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                ScheduleTimer(TimerName).OnStartFailed(e => workflowAction);
            }
        }
    }
}