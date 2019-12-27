using System;
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
        private const string Version = "1.0";
        private const string PromotoEmployee = "PromotoEmployee";
        private const string PromotionConfirmed = "PromotionConfirmed";
        private const string HRApprovalTimedout = "HRApprovalTimedout";
        private const string ManagerApprovalTimedout = "ManagerApprovalTimedout";
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
            var completedStamp = DateTime.UtcNow.AddHours(-4);
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
            var completionDate = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completionDate);
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, completedEventId, completionDate, _waitingSignals));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(2), TimerType.SignalTimer, completedEventId);
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(promoteEmployee, completedEventId, _waitingSignals, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal(
            Type workflowType, ScheduleId promoteEmployee, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
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
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "HRApproved"));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(promoteEmployee, signalTriggerEventId, "ManagerApproved"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
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
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "", DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var managerApproved = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "", DateTime.UtcNow.AddHours(-1));
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
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completeDateTime, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(managerSignal);

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

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
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completeDateTime, _waitingSignals));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, DateTime.UtcNow.AddHours(-2));
            _builder.AddNewEvents(timerFiredGraph);

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: DateTime.UtcNow.AddHours(-2)));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: DateTime.UtcNow.AddHours(-2)));
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
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(promoteEmployee, signalTriggerEventId, completedStamp, _waitingSignals));
            var hrSignal = _graphBuilder.WorkflowSignaledEvent("HRApproved", "input", completedTime: DateTime.UtcNow);
            _builder.AddNewEvents(hrSignal);
            var managerSignal = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "input", completedTime: DateTime.UtcNow);
            _builder.AddNewEvents(managerSignal);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(promoteEmployee, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, DateTime.UtcNow);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(3));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_hrApprovalTimedout, "")));
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_managerApprovalTimedout, "")));
            decisions[2].AssertSignalTimedout(promoteEmployee, signalTriggerEventId, _waitingSignals, hrSignal.EventId);
        }


        private static HistoryEvent[] ActivityCompletedEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.ActivityCompletedGraph(id, "input", "res", completedStamp: completeDateTime)
                .ToArray();
        }

        private static HistoryEvent[] LambdaCompletedEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.LambdaCompletedEventGraph(id, "input", "res", completedStamp: completeDateTime)
                .ToArray();
        }

        private static HistoryEvent[] ChildWorkflowCompletedEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.ChildWorkflowCompletedGraph(id, "rid", "input", "res", completionStamp: completeDateTime)
                .ToArray();
        }

        private static HistoryEvent[] TimerFiredEventGraph(ScheduleId id, DateTime completeDateTime)
        {
            return _graphBuilder.TimerFiredGraph(id, TimeSpan.FromSeconds(10), TimerType.WorkflowItem, timerFiredDateTime: completeDateTime)
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

            var lambdaId = Identity.Lambda(PromotoEmployee).ScheduleId();
            var l = new CompletedEventGraph(e => LambdaCompletedEventGraph(lambdaId, e));
            yield return new TestCaseData(typeof(PromoteWorkflowWithLambda), lambdaId, l);
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck), lambdaId, l);

            //yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI), lambdaId, l);
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI), lambdaId, l);

            //var activityId = Identity.New(PromotoEmployee, Version).ScheduleId();
            //var a = new WaitForAnySignalActionWithTimeoutTests.CompletedEventGraph(e => ActivityCompletedEventGraph(activityId, e));
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithActivityAndApprovedTimedoutCheck), activityId, a);
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi), activityId, a);

            //var workflowId = Identity.New(PromotoEmployee, Version).ScheduleId(WorkflowRunId);
            //var c = new WaitForAnySignalActionWithTimeoutTests.CompletedEventGraph(e => ChildWorkflowCompletedEventGraph(workflowId, e));
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck), workflowId, c);
            //yield return new TestCaseData(typeof(ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI), workflowId, c);

            //var timerId = Identity.Timer(PromotoEmployee).ScheduleId();
            //var t = new WaitForAnySignalActionWithTimeoutTests.CompletedEventGraph(e => TimerFiredEventGraph(timerId, e));
            //yield return new TestCaseData(typeof(ApproveWorkflowWithTimerAndApprovedTimeoutCheck), timerId, t);
            //yield return new TestCaseData(typeof(ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI), timerId, t);
        }

        private class PromoteWorkflowWithLambda : Workflow
        {
            public PromoteWorkflowWithLambda()
            {
                ScheduleLambda(PromotoEmployee)
                    .OnCompletion(e => e.WaitForAllSignals("HRApproved", "ManagerApproved").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("HRApproved").IsTriggered()|| Signal("ManagerApproved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("HRApproved").IsTimedout());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("ManagerApproved").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck : Workflow
        {
            public ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck()
            {
                ScheduleLambda(PromotoEmployee)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI()
            {
                ScheduleLambda(PromotoEmployee)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(l => l.ParentLambda().IsSignalTimedout("Approved"));
            }
        }

        private class ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI()
            {
                ScheduleLambda(PromotoEmployee)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterLambda(PromotoEmployee)
                    .When(l => l.ParentLambda().IsSignalTimedout("Rejected"));
            }
        }

        private class ApprovalWorkflowWithActivityAndApprovedTimedoutCheck : Workflow
        {
            public ApprovalWorkflowWithActivityAndApprovedTimedoutCheck()
            {
                ScheduleActivity(PromotoEmployee, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterActivity(PromotoEmployee, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(_ => Signal("Approved").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi : Workflow
        {
            public ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi()
            {
                ScheduleActivity(PromotoEmployee, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterActivity(PromotoEmployee, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(l => l.ParentActivity().IsSignalTimedout("Rejected"));
            }
        }

        private class ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck : Workflow
        {
            public ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck()
            {
                ScheduleChildWorkflow(PromotoEmployee, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterChildWorkflow(PromotoEmployee, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterChildWorkflow(PromotoEmployee, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalTimedout("Approved"));
            }
        }

        private class ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI()
            {
                ScheduleChildWorkflow(PromotoEmployee, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterChildWorkflow(PromotoEmployee, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterChildWorkflow(PromotoEmployee, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterActivity(PromotoEmployee, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalTimedout("Rejected"));
            }
        }


        private class ApproveWorkflowWithTimerAndApprovedTimeoutCheck : Workflow
        {
            public ApproveWorkflowWithTimerAndApprovedTimeoutCheck()
            {
                ScheduleTimer(PromotoEmployee).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterTimer(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterTimer(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterTimer(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTimedout());
            }
        }

        private class ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI : Workflow
        {
            public ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI()
            {
                ScheduleTimer(PromotoEmployee).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(PromotionConfirmed).AfterTimer(PromotoEmployee)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(HRApprovalTimedout).AfterTimer(PromotoEmployee)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ManagerApprovalTimedout).AfterTimer(PromotoEmployee)
                    .When(l => l.ParentTimer().IsSignalTimedout("Rejected"));
            }
        }
    }
}