﻿using System;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
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

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal_when_the_lambda_completed_event_is_processed_immediately(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow;
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow) Activator.CreateInstance(workflowType);
            var lambdaCompletedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, completedStamp,TimeSpan.FromHours(2),"Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(2));
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Reduce_signal_timer_timeout_by_an_hour_when_execution_of_wait_for_signal_workflow_action_is_delayed_by_one_hour(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-1);
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow) Activator.CreateInstance(workflowType);
            var lambdaCompletedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();


            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, completedStamp,TimeSpan.FromHours(2),"Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(1));
        }


        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Returns_the_timer_decision_to_fire_immediately_and_decision_to_wait_for_a_signal_when_lambda_completed_event_is_processed_after_timeout(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow) Activator.CreateInstance(workflowType);
            var lambdaCompletedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(_confirmEmailId, lambdaCompletedEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, lambdaCompletedEventId, TimeSpan.FromHours(0));
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Signal_is_ignored_when_it_come_after_wait_is_timedout_by_signal_timer(Type workflowType)
        {
            var completionDate = DateTime.UtcNow.AddHours(-4);
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completionDate);
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, completedEventId, completionDate, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(2), TimerType.SignalTimer, completedEventId);
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(_confirmEmailId, completedEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, completedStamp, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "input")));
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, completedStamp, "Confirmed"));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, signalTriggerEventId, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Return_timer_decision_and_continue_execution_with_signal_triggered_when_signal_and_lambda_completed_events_comes_together_and_signal_come_before_timeout(Type workflowType)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-1);
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", currentTime);
            _builder.AddNewEvents(s);
            var workflow = (Workflow) Activator.CreateInstance(workflowType);
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(4));
            decisions[0].AssertWaitForSignal(_confirmEmailId, triggerEventId, completedStamp,TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[3], Is.EqualTo(new WorkflowItemSignalledDecision(_confirmEmailId, triggerEventId, "Confirmed", s.EventId)));
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Return_timer_decision_and_continue_execution_with_signal_timedout_when_signal_and_lambda_completed_events_comes_together_but_signal_comes_after_timeout(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = LambdaCompletedEventGraph(_confirmEmailId, completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(4));

            var triggerEventId = graph.First().EventId;
            decisions[0].AssertWaitForSignal(_confirmEmailId, triggerEventId, completedStamp, TimeSpan.FromHours(2),"Confirmed");
            decisions[1].AssertSignalTimer(_confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
            decisions[3].AssertSignalTimedout(_confirmEmailId, triggerEventId, new[] { "Confirmed" }, s.EventId);
        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together(Type workflowType)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg =LambdaCompletedEventGraph(_confirmEmailId, completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, completeDateTime, "Confirmed"));
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[1], Is.EqualTo(new WorkflowItemSignalledDecision(_confirmEmailId, signalTriggerEventId, "Confirmed", s.EventId)));

        }

        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Signal_is_ignored_when_it_is_received_after_signal_timer_and_signal_timer_and_signal_are_processed_together(Type workflowType)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = LambdaCompletedEventGraph(_confirmEmailId, completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, signalTriggerEventId, completeDateTime, "Confirmed"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);

            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var workflow = (Workflow) Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(_confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
        }


        [TestCase(typeof(UserActivateWorkflow))]
        [TestCase(typeof(UserActivateWorkflowCheckWithAPI))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together_also_signal_was_received_after_timedout(Type workflowType)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = LambdaCompletedEventGraph(_confirmEmailId,  completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(_confirmEmailId, signalTriggerEventId,completedStamp, "Confirmed"));
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow);
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(_confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
            decisions[1].AssertSignalTimedout(_confirmEmailId, signalTriggerEventId, new []{"Confirmed"}, s.EventId);
        }


        private HistoryEvent[] LambdaCompletedEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.LambdaCompletedEventGraph(id, "input", "res", completedStamp: completeDateTime)
                .ToArray();
        }

        private HistoryEvent WaitForSignalEvent(ScheduleId id, long signalTriggerEventId, DateTime triggerCompletionDateTime, params string[] signals)
        {
            return _graphBuilder.WaitForSignalEvent(id, signalTriggerEventId, signals, SignalWaitType.Any,
                SignalNextAction.Continue, triggerCompletionDateTime, TimeSpan.FromHours(2));
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

        private class UserActivateWorkflowCheckWithAPI : Workflow
        {
            public UserActivateWorkflowCheckWithAPI()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail")
                    .When(l => l.ParentLambda().IsSignalled("Confirmed"));

                ScheduleLambda("BlockAccount").AfterLambda("ConfirmEmail")
                    .When(l => l.ParentLambda().IsSignalTimedout("Confirmed"));
            }
        }


    }
}