﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForAllSignalsActionWithTimeoutTests
    {
        private HistoryEventsBuilder _builder;
        private static EventGraphBuilder _graphBuilder;
        private ScheduleId _promotionConfirmed;
        private ScheduleId _hrApprovalTimedout;
        private ScheduleId _managerApprovalTimedout;
        private ScheduleId _promotionTimedout;
        private const string PromoteEmployee = "PromoteEmployee";
        private const string PromotionConfirmed = "PromotionConfirmed";
        private const string HRApprovalTimedout = "HRApprovalTimedout";
        private const string ManagerApprovalTimedout = "ManagerApprovalTimedout";
        private const string PromotionTimedout = "PromotionTimedout";
        private const string WorkflowRunId = "id";
        private string[] _waitingSignals = {"HRApproved", "ManagerApproved"};
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _promotionConfirmed = Identity.Lambda(PromotionConfirmed).ScheduleId();
            _hrApprovalTimedout = Identity.Lambda(HRApprovalTimedout).ScheduleId();
            _managerApprovalTimedout = Identity.Lambda(ManagerApprovalTimedout).ScheduleId();
            _promotionTimedout = Identity.Lambda(PromotionTimedout).ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddWorkflowRunId(WorkflowRunId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal_when_the_workflow_item_completed_event_is_processed_immediately
                    (Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow;
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForAllSignals(promoteEmployee, completedEventId, completedStamp, TimeSpan.FromHours(2), _waitingSignals);
            decisions[1].AssertSignalTimer(promoteEmployee, completedEventId, TimeSpan.FromHours(2));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Reduce_signal_timer_timeout_by_an_hour_when_execution_of_wait_for_signal_workflow_action_is_delayed_by_one_hour(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-1);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();


            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForAllSignals(approveExpenses, completedEventId, completedStamp, TimeSpan.FromHours(2), _waitingSignals);
            decisions[1].AssertSignalTimer(approveExpenses, completedEventId, TimeSpan.FromHours(1));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_to_fire_immediately_and_decision_to_wait_for_a_signal_when_workflow_item_completed_event_is_processed_after_timeout(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForAllSignals(promoteEmployee, completedEventId, completedStamp, TimeSpan.FromHours(2), _waitingSignals);
            decisions[1].AssertSignalTimer(promoteEmployee, completedEventId, TimeSpan.FromHours(0));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signals_are_ignored_when_they_come_after_wait_is_timedout_by_signal_timer(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completionDate = currentTime.AddHours(-4);
            var graph = completedGraph(completionDate);
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, completedEventId, completionDate, _waitingSignals));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(2), TimerType.SignalTimer, completedEventId, currentTime.AddHours(-2));
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(promoteEmployee, completedEventId, _waitingSignals, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", currentTime));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", currentTime));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);
            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(3));
            decisions[0].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, _waitingSignals, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_hrApprovalTimedout, "input")));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "input")));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "HRApproved"));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "ManagerApproved"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId,currentTime);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

       [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_triggered_when_signals_and_workflow_item_completed_events_comes_together_and_signal_come_before_timeout(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-1);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "", currentTime);
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "", currentTime);
            _builder.AddNewEvents(managerSignal);


            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(5));
            decisions[0].AssertWaitForAllSignals(promoteEmployee, triggerEventId, completedStamp, TimeSpan.FromHours(2), _waitingSignals);
            decisions[1].AssertSignalTimer(promoteEmployee, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, triggerEventId, "HRApproved", hrSignal.EventId)));
            Assert.That(decisions[3], Is.EqualTo(new ScheduleLambdaDecision(_promotionConfirmed, "")));
            Assert.That(decisions[4], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, triggerEventId, "ManagerApproved", managerSignal.EventId)));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_timedout_when_signal_and_workflow_item_completed_events_comes_together_but_signals_comes_after_timeout(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "", currentTime.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var managerApproved = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "", currentTime.AddHours(-1));
            _builder.AddNewEvents(managerApproved);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(5));

            var triggerEventId = graph.First().EventId;
            decisions[0].AssertWaitForAllSignals(promoteEmployee, triggerEventId, completedStamp, TimeSpan.FromHours(2), _waitingSignals);
            decisions[1].AssertSignalTimer(promoteEmployee, triggerEventId, TimeSpan.FromHours(0));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_hrApprovalTimedout, "")));
            Assert.That(decisions[3], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
            decisions[4].AssertSignalTimedout(promoteEmployee, triggerEventId, _waitingSignals, hrSignal.EventId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signals_and_signal_timer_and_signals_are_processed_together(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completeDateTime = currentTime.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completeDateTime, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime.AddHours(-1));
            _builder.AddNewEvents(managerSignal);

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            Assert.That(decisions[0], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, signalTriggerEventId, "HRApproved", hrSignal.EventId)));
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_promotionConfirmed, "")));
            Assert.That(decisions[2], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, signalTriggerEventId, "ManagerApproved", managerSignal.EventId)));

        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signals_are_ignored_when_they_are_received_after_signal_timer_and_signal_timer_and_signals_are_processed_together(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completeDateTime = currentTime.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completeDateTime, _waitingSignals));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);
            _builder.AddNewEvents(timerFiredGraph);

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddSeconds(1)));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime.AddSeconds(1)));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            decisions[0].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, _waitingSignals, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_hrApprovalTimedout, "")));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signals_and_signal_timer_and_signals_are_processed_together_also_signal_was_received_after_timedout(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddMinutes(1));
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime.AddMinutes(1));
            _builder.AddNewEvents(managerSignal);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime.AddMinutes(1));

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_hrApprovalTimedout, "")));
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
            decisions[2].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, _waitingSignals, hrSignal.EventId);
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Continue_with_manager_approval_timeout_when_hrapproval_is_received_before_timeout_and_manager_approval_is_received_after_timeout_and_both_signals_are_processed_togather(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddMinutes(-121);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime);
            _builder.AddNewEvents(managerSignal);
           
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            Assert.That(decisions[0], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, signalTriggerEventId, "HRApproved", hrSignal.EventId)));
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
            decisions[2].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, new[] { "ManagerApproved" }, managerSignal.EventId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Continue_with_manager_approval_timeout_when_hrapproval_is_already_received_and_processed_before_timeout_and_manager_approval_is_received_after_timeout(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddMinutes(-121);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "HRApproved"));
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: currentTime);
            _builder.AddNewEvents(managerSignal);

            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
            decisions[1].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, new[] { "ManagerApproved" }, managerSignal.EventId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Continue_with_manager_approval_timeout_when_hrapproval_is_received_before_timeout_and_signal_timer_is_fired_and_both_signal_and_signal_timer_are_processed_togather(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);

            _builder.AddNewEvents(timerFiredGraph);

            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            Assert.That(decisions[0], Is.EqualTo(new WorkflowItemSignalledDecision(promoteEmployee, signalTriggerEventId, "HRApproved", hrSignal.EventId)));
            decisions[1].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, new[] { "ManagerApproved" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Continue_with_manager_approval_timeout_when_hrapproval_is_received_and_processed_before_timeout_and_signal_timer_is_fired(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "HRApproved"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);

            _builder.AddNewEvents(timerFiredGraph);

            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, new[] { "ManagerApproved" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
        }

        [Test]
        public void Continue_with_manager_approval_timeout_when_hrapproval_is_received_and_processed_before_timeout_and_signal_timer_is_fired_and_using_any_signal_api()
        {
            var promoteEmployee = Identity.Lambda(PromoteEmployee).ScheduleId();
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-2);
            var lg = LambdaCompletedEventGraph(promoteEmployee, completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: currentTime.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "HRApproved"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, currentTime);

            _builder.AddNewEvents(timerFiredGraph);

            var workflow = new PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAndTimedoutAPI();

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, new[] { "ManagerApproved" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_promotionTimedout, "")));
        }

        private static HistoryEvent[] LambdaCompletedEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.LambdaCompletedEventGraph(id, "input", "res", completedStamp: completeDateTime)
                .ToArray();
        }

        private HistoryEvent WaitForSignalEvent(ScheduleId id, long signalTriggerEventId, DateTime triggerCompletionDateTime, params string[] signals)
        {
            return _graphBuilder.WaitForSignalEvent(id, signalTriggerEventId, signals, SignalWaitType.All,
                SignalNextAction.Continue, triggerCompletionDateTime, TimeSpan.FromHours(2));
        }


        public delegate HistoryEvent[] CompletedEventGraph(DateTime completionDate);

        private static IEnumerable<TestCaseData> TestCaseData()
        {

            var lambdaId = Identity.Lambda(PromoteEmployee).ScheduleId();
            var l = new CompletedEventGraph(e => LambdaCompletedEventGraph(lambdaId, e));
            yield return new TestCaseData(typeof(PromoteWorkflowWithLambda), lambdaId, l);
            yield return new TestCaseData(typeof(PromoteWorkflowWithLambdaUsingAPI), lambdaId, l);
            yield return new TestCaseData(typeof(PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAPI), lambdaId, l);
        }

        private class PromoteWorkflowWithLambda : Workflow
        {
            public PromoteWorkflowWithLambda()
            {
                ScheduleLambda(PromoteEmployee)
                    .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromoteEmployee)
                    .When(_ => Signal("HRApproved").IsTriggered()|| Signal("ManagerApproved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(_ => Signal("HRApproved").IsTimedout());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(_ => Signal("ManagerApproved").IsTimedout());
            }
        }

        private class PromoteWorkflowWithLambdaUsingAPI : Workflow
        {
            public PromoteWorkflowWithLambdaUsingAPI()
            {
                ScheduleLambda(PromoteEmployee)
                    .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromoteEmployee)
                    .When(l => l.ParentLambda().IsSignalled("HRApproved") && l.ParentLambda().IsSignalled("ManagerApproved"));

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(l => l.ParentLambda().IsSignalTimedout("HRApproved"));

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(l => l.ParentLambda().IsSignalTimedout("ManagerApproved"));
            }
        }

        private class PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAPI : Workflow
        {
            public PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAPI()
            {
                ScheduleLambda(PromoteEmployee)
                    .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromoteEmployee)
                    .When(_ => AnySignal("HRApproved","ManagerApproved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(_ => Signal("HRApproved").IsTimedout());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromoteEmployee)
                    .When(_ => Signal("ManagerApproved").IsTimedout());
            }
        }

        private class PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAndTimedoutAPI : Workflow
        {
            public PromoteWorkflowWithLambdaUsingAnySignalIsTriggeredAndTimedoutAPI()
            {
                ScheduleLambda(PromoteEmployee)
                    .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromoteEmployee)
                    .When(_ => AnySignal("HRApproved", "ManagerApproved").IsTriggered());

                ScheduleLambda(PromotionTimedout).AfterLambda(PromoteEmployee)
                    .When(_ => AnySignal("HRApproved", "ManagerApproved").IsTimedout());
            }
        }

    }
}