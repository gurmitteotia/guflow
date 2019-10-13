using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WaitForSignalActionWithTimeoutTests
    {
        private HistoryEventsBuilder _builder;
        private EventGraphBuilder _graphBuilder;
        private ScheduleId _confirmEmailId;
        private ScheduleId _activateUserId;
        private ScheduleId _blockAccountId;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _confirmEmailId = Identity.Lambda("ConfirmEmail").ScheduleId();
            _activateUserId = Identity.Lambda("ActivateUser").ScheduleId();
            _blockAccountId = Identity.Lambda("BlockAccount").ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
        }

        [Test]
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp:DateTime.UtcNow);
            _builder.AddNewEvents(graph);
            var workflow = new UserActivateWorkflow();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var lambdaCompletedEventId = graph.First().EventId;

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(2));
        }

        [Test]
        public void Do_not_return_timer_decision_when_signal_and_lambda_completed_events_comes_together_and_signal_come_before_timeout()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp:DateTime.UtcNow.AddMinutes(-20));
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", DateTime.UtcNow);
            _builder.AddNewEvents(s);
            var workflow = new UserActivateWorkflow();
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(3));
            decisions[0].AssertWaitForSignal(_confirmEmailId, triggerEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[2], Is.EqualTo(new WorkflowItemSignalledDecision(_confirmEmailId, triggerEventId, "Confirmed", s.EventId)));
        }

        [Test]
        public void Reduce_signal_timer_timeout_by_an_hour_when_execution_of_wait_for_signal_workflow_action_is_delayed_by_one_hour()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = new UserActivateWorkflow();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var lambdaCompletedEventId = graph.First().EventId;

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(1));
        }

        //[Test]
        //public void Do_not_return_timer_decision_when_signal_and_lambda_completed_events_comes_together_and_signal_after_timeout()
        //{
        //    var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow.AddHours(-3));
        //    _builder.AddNewEvents(graph);
        //    var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", DateTime.UtcNow);
        //    _builder.AddNewEvents(s);
        //    var workflow = new UserActivateWorkflow();
        //    var decisions = workflow.Decisions(_builder.Result()).ToArray();

        //    Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
        //    {
        //        new WaitForSignalsDecision(new WaitForSignalData{ScheduleId = _confirmEmailId, TriggerEventId = graph.First().EventId}),
        //        new ScheduleLambdaDecision(_blockAccountId, ""),
        //        new WorkflowItemSignalledDecision(_confirmEmailId, graph.First().EventId, "Confirmed", s.EventId),
        //    }));
        //}

        private class UserActivateWorkflow : Workflow
        {
            public UserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail")
                    .When(_ => Signal("Confirmed").IsTriggered());

                //ScheduleLambda("BlockAccount").AfterLambda("ConfirmEmail")
                //    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }
    }
}