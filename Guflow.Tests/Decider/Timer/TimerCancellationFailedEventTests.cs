// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Amazon.SimpleWorkflow.Model;
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
        private const string WorkflowName = "Workflow";
        private const string WorkflowVersion = "1.0";
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Identity _id;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _id = Identity.Timer(TimerName);
            _timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(_id, Cause);
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
            _builder.AddNewEvents(_graphBuilder.TimerCancellationFailedGraph(_id.ScheduleId(), Cause).ToArray());
            var decisions = new TestWorkflow().Decisions(_builder.Result());
            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("TIMER_CANCELLATION_FAILED",Cause)}));
        }

        [Test]
        public void Fail_workflow_for_activity_reschedule_timer()
        {
            var workflow = new WorkflowWithActivity();
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.New(ActivityName, ActivityVersion), Cause);
            var decisions = timerCancellationFailedEvent.Interpret(workflow).Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new []{new FailWorkflowDecision("RESCHEDULE_TIMER_CANCELLATION_FAILED", Cause)}));
        }

        [Test]
        public void Fail_workflow_for_lambda_reschedule_timer()
        {
            var workflow = new WorkflowWithLambda();
            var timerCancellationFailedEvent = CreateTimerCancellationFailedEvent(Identity.Lambda(LambdaName), Cause);
            var decisions = timerCancellationFailedEvent.Interpret(workflow).Decisions(Mock.Of<IWorkflow>());

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_CANCELLATION_FAILED", Cause) }));
        }
        [Test]
        public void Fail_workflow_for_child_workflow_reschedule_timer()
        {
            const string workflowRunid = "rid";
            var builder = new HistoryEventsBuilder().AddWorkflowRunId(workflowRunid);
            builder.AddNewEvents(TimerCancellationFailedEventGrpah(Identity.New(WorkflowName, WorkflowVersion).ScheduleId(workflowRunid), Cause));
            
            var decisions = new WorkflowWithChildWorkflow().Decisions(builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new FailWorkflowDecision("RESCHEDULE_TIMER_CANCELLATION_FAILED", Cause) }));
        }

        [Test]
        public void Can_return_custom_workflow_action_from_workflow()
        {
            _builder.AddNewEvents(_graphBuilder.TimerCancellationFailedGraph(_id.ScheduleId(), Cause).ToArray());
            var decisions = new WorkflowWithCustomAction("result").Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []{new CompleteWorkflowDecision("result")}));
        }
        
        private TimerCancellationFailedEvent CreateTimerCancellationFailedEvent(Identity id, string cause)
        {
            var timerCancellationFailedEventGraph = _graphBuilder.TimerCancellationFailedGraph(id.ScheduleId(), Cause);
            return new TimerCancellationFailedEvent(timerCancellationFailedEventGraph.First());
        }
        private HistoryEvent[] TimerCancellationFailedEventGrpah(ScheduleId identity, string cause)
        {
            return _graphBuilder.TimerCancellationFailedGraph(identity, Cause).ToArray();
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
            public WorkflowWithCustomAction(string result)
            {
                ScheduleTimer(TimerName).OnCancellationFailed(c => CompleteWorkflow(result));
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
        private class WorkflowWithChildWorkflow : Workflow
        {
            public WorkflowWithChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion);
            }
        }
    }
}