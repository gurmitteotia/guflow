// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Amazon.SimpleWorkflow.Model;
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

        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Identity _identity;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _identity = Identity.Timer(TimerName);
            _timerStartFailedEvent = CreateTimerStartFailedEvent(_identity, TimerFailureCause);
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
            _builder.AddNewEvents(_graphBuilder.TimerStartFailedGraph(_identity.ScheduleId(), TimerFailureCause).ToArray());
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("TIMER_START_FAILED", TimerFailureCause)}));
        }

        [Test]
        public void Can_return_the_custom_action()
        {
            _builder.AddNewEvents(_graphBuilder.TimerStartFailedGraph(_identity.ScheduleId(), "cause").ToArray());
            var workflow = new WorkflowWithCustomAction("result");

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions,Is.EqualTo(new []{new CompleteWorkflowDecision("result")}));
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
            const string workflowRunid = "rid";
            var identity = Identity.New(WorkflowName, WorkflowVersion).ScheduleId(workflowRunid);
            var builder = new HistoryEventsBuilder().AddWorkflowRunId(workflowRunid);
            builder.AddNewEvents(TimerStartFailedEventGraph(identity, TimerFailureCause));

            var decisions = new WorkflowWithChildWorkflow().Decisions(builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_START_FAILED", TimerFailureCause) }));
        }
        private TimerStartFailedEvent CreateTimerStartFailedEvent(Identity timerIdentity, string cause)
        {
            var timerFailedEventGraph = _graphBuilder.TimerStartFailedGraph(timerIdentity.ScheduleId(), cause);
            return new TimerStartFailedEvent(timerFailedEventGraph.First());
        }

        private HistoryEvent[] TimerStartFailedEventGraph(ScheduleId timerIdentity, string cause)
        {
            return _graphBuilder.TimerStartFailedGraph(timerIdentity, cause).ToArray();
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
            public WorkflowWithCustomAction(string result)
            {
                ScheduleTimer(TimerName).OnStartFailed(e => CompleteWorkflow(result));
            }
        }
    }
}