using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
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
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal_when_the_lambda_completed_event_is_processed_immediately()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = new UserActivateWorkflow();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var lambdaCompletedEventId = graph.First().EventId;

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(2));
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


        [Test]
        public void Returns_the_timer_decision_to_fire_immediately_and_decision_to_wait_for_a_signal_when_lambda_completed_event_is_processed_after_timeout()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow.AddHours(-4));
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = new UserActivateWorkflow();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var lambdaCompletedEventId = graph.First().EventId;

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(0));
        }

        [Test]
        public void Signal_is_ignored_when_it_come_after_wait_is_timedout_by_signal_timer()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow.AddHours(-4));
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, completedEventId, new[] { "Confirmed" }, SignalWaitType.Any));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(2),
                TimerType.SignalTimer, completedEventId);
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(_confirmEmailId, completedEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = new UserActivateWorkflow();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal()
        {
            var workflow = new UserActivateWorkflow();
            var lg = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "i", "r", TimeSpan.FromSeconds(1), DateTime.UtcNow.AddHours(-2));
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, SignalWaitType.Any));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "input")));
        }
       
        [Test]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal()
        {
            var workflow = new UserActivateWorkflow();
            var lg = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "i", "r", TimeSpan.FromSeconds(1), DateTime.UtcNow.AddHours(-2));
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, signalTriggerEventId, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Return_timer_decision_and_continue_execution_with_signal_triggered_when_signal_and_lambda_completed_events_comes_together_and_signal_come_before_timeout()
        {
            var currentTime = DateTime.UtcNow;
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: currentTime.AddHours(-1));
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", currentTime);
            _builder.AddNewEvents(s);
            var workflow = new UserActivateWorkflow();
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(4));
            decisions[0].AssertWaitForSignal(_confirmEmailId, triggerEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[3], Is.EqualTo(new WorkflowItemSignalledDecision(_confirmEmailId, triggerEventId, "Confirmed", s.EventId)));
        }

        [Test]
        public void Return_timer_decision_and_continue_execution_with_signal_timedout_when_signal_and_lambda_completed_events_comes_together_but_signal_comes_after_timeout()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result", completedStamp: DateTime.UtcNow.AddHours(-4));
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = new UserActivateWorkflow();
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(4));

            var triggerEventId = graph.First().EventId;
            decisions[0].AssertWaitForSignal(_confirmEmailId, triggerEventId, SignalWaitType.Any, SignalNextAction.Continue, "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
            decisions[3].AssertSignalTimedout(_confirmEmailId, triggerEventId, new[] { "Confirmed" }, s.EventId);
        }
     
        [Test]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together()
        {
            var workflow = new UserActivateWorkflow();
            var lg = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "i", "r", TimeSpan.FromSeconds(1), DateTime.UtcNow.AddHours(-2));
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, SignalWaitType.Any));
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[1], Is.EqualTo(new WorkflowItemSignalledDecision(_confirmEmailId, signalTriggerEventId, "Confirmed", s.EventId)));

        }

        [Test]
        public void Signal_is_ignored_when_it_is_received_after_signal_timer_and_signal_timer_and_signal_are_processed_together()
        {
            var workflow = new UserActivateWorkflow();
            var lg = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "i", "r", TimeSpan.FromSeconds(1), DateTime.UtcNow.AddHours(-2));
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, SignalWaitType.Any));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);

            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
        }

        private class UserActivateWorkflow : Workflow
        {
            public UserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail")
                    .When(_ => Signal("Confirmed").IsTriggered());

                ScheduleLambda("BlockAccount").AfterLambda("ConfirmEmail")
                    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }


    }
}