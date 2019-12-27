using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForAnySignalActionWithTimeoutTests
    {
        private HistoryEventsBuilder _builder;
        private static EventGraphBuilder _graphBuilder;
        private ScheduleId _submitToAccount;
        private ScheduleId _sendBackToEmployee;
        private ScheduleId _approvalTimedout;
        private const string Version = "1.0";
        private const string ApproveExpense = "ApproveExpense";
        private const string SubmitToAccount = "SubmitToAccount";
        private const string SendBackToEmployee = "SendBackToEmployee";
        private const string ApprovalTimedout = "ApprovalTimedout";
        private const string WorkflowRunId = "id";
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _submitToAccount = Identity.Lambda(SubmitToAccount).ScheduleId();
            _sendBackToEmployee = Identity.Lambda(SendBackToEmployee).ScheduleId();
            _approvalTimedout = Identity.Lambda(ApprovalTimedout).ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddWorkflowRunId(WorkflowRunId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal_when_the_workflow_item_completed_event_is_processed_immediately
                    (Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow;
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForAnySignal(approveExpenses, completedEventId, completedStamp, TimeSpan.FromHours(2), "Approved", "Rejected");
            decisions[1].AssertSignalTimer(approveExpenses, completedEventId, TimeSpan.FromHours(2));
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
            decisions[0].AssertWaitForAnySignal(approveExpenses, completedEventId, completedStamp, TimeSpan.FromHours(2), "Approved", "Rejected");
            decisions[1].AssertSignalTimer(approveExpenses, completedEventId, TimeSpan.FromHours(1));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_to_fire_immediately_and_decision_to_wait_for_a_signal_when_workflow_item_completed_event_is_processed_after_timeout(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForAnySignal(approveExpenses, completedEventId, completedStamp, TimeSpan.FromHours(2), "Approved", "Rejected");
            decisions[1].AssertSignalTimer(approveExpenses, completedEventId, TimeSpan.FromHours(0));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_is_ignored_when_it_come_after_wait_is_timedout_by_signal_timer(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completionDate = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completionDate);
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, completedEventId, completionDate, "Approved", "Rejected"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(2), TimerType.SignalTimer, completedEventId);
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(approveExpenses, completedEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, signalTriggerEventId, completedStamp, "Approved", "Rejected"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(approveExpenses, signalTriggerEventId, new[] { "Approved", "Rejected" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_approvalTimedout, "input")));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, signalTriggerEventId, completedStamp, "Approved", "Rejected"));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Approved", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(approveExpenses, signalTriggerEventId, "Approved"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_triggered_when_signal_and_workflow_item_completed_events_comes_together_and_signal_come_before_timeout(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-1);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Rejected", "", currentTime);
            _builder.AddNewEvents(s);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(4));
            decisions[0].AssertWaitForAnySignal(approveExpenses, triggerEventId, completedStamp, TimeSpan.FromHours(2), "Approved", "Rejected");
            decisions[1].AssertSignalTimer(approveExpenses, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_sendBackToEmployee, "")));
            Assert.That(decisions[3], Is.EqualTo(new WorkflowItemSignalledDecision(approveExpenses, triggerEventId, "Rejected", s.EventId)));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_timedout_when_signal_and_workflow_item_completed_events_comes_together_but_signal_comes_after_timeout(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Rejected", "", DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(4));

            var triggerEventId = graph.First().EventId;
            decisions[0].AssertWaitForAnySignal(approveExpenses, triggerEventId, completedStamp, TimeSpan.FromHours(2), "Approved", "Rejected");
            decisions[1].AssertSignalTimer(approveExpenses, triggerEventId, TimeSpan.FromHours(0));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_approvalTimedout, "")));
            decisions[3].AssertSignalTimedout(approveExpenses, triggerEventId, new[] { "Approved", "Rejected" }, s.EventId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, signalTriggerEventId, completeDateTime, "Approved", "Rejected"));
            var s = _graphBuilder.WorkflowSignaledEvent("Approved", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_submitToAccount, "")));
            Assert.That(decisions[1], Is.EqualTo(new WorkflowItemSignalledDecision(approveExpenses, signalTriggerEventId, "Approved", s.EventId)));

        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_is_ignored_when_it_is_received_after_signal_timer_and_signal_timer_and_signal_are_processed_together(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, signalTriggerEventId, completeDateTime, "Approved", "Rejected"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId, DateTime.UtcNow.AddHours(-2));
            _builder.AddNewEvents(timerFiredGraph);

            var s = _graphBuilder.WorkflowSignaledEvent("Approved", "input", completedTime: DateTime.UtcNow.AddHours(-2));
            _builder.AddNewEvents(s);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(approveExpenses, signalTriggerEventId, new[] { "Approved", "Rejected" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_approvalTimedout, "")));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together_also_signal_was_received_after_timedout(
            Type workflowType, ScheduleId approveExpenses, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(approveExpenses, signalTriggerEventId, completedStamp, "Approved", "Rejected"));
            var s = _graphBuilder.WorkflowSignaledEvent("Approved", "input", completedTime: DateTime.UtcNow);
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(approveExpenses, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId,DateTime.UtcNow);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_approvalTimedout, "")));
            decisions[1].AssertSignalTimedout(approveExpenses, signalTriggerEventId, new[] { "Approved", "Rejected" }, s.EventId);
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
            return _graphBuilder.WaitForSignalEvent(id, signalTriggerEventId, signals, SignalWaitType.Any,
                SignalNextAction.Continue, triggerCompletionDateTime, TimeSpan.FromHours(2));
        }


        public delegate HistoryEvent[] CompletedEventGraph(DateTime completionDate);

        private static IEnumerable<TestCaseData> TestCaseData()
        {

            var lambdaId = Identity.Lambda(ApproveExpense).ScheduleId();
            var l = new CompletedEventGraph(e => LambdaCompletedEventGraph(lambdaId, e));
            yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndApprovedTimedoutCheck), lambdaId, l);
            yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck), lambdaId, l);

            yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI), lambdaId, l);
            yield return new TestCaseData(typeof(ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI), lambdaId, l);

            var activityId = Identity.New(ApproveExpense, Version).ScheduleId();
            var a = new CompletedEventGraph(e => ActivityCompletedEventGraph(activityId, e));
            yield return new TestCaseData(typeof(ApprovalWorkflowWithActivityAndApprovedTimedoutCheck), activityId, a);
            yield return new TestCaseData(typeof(ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi), activityId, a);

            var workflowId = Identity.New(ApproveExpense, Version).ScheduleId(WorkflowRunId);
            var c = new CompletedEventGraph(e => ChildWorkflowCompletedEventGraph(workflowId, e));
            yield return new TestCaseData(typeof(ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck), workflowId, c);
            yield return new TestCaseData(typeof(ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI), workflowId, c);

            var timerId = Identity.Timer(ApproveExpense).ScheduleId();
            var t = new CompletedEventGraph(e => TimerFiredEventGraph(timerId, e));
            yield return new TestCaseData(typeof(ApproveWorkflowWithTimerAndApprovedTimeoutCheck), timerId, t);
            yield return new TestCaseData(typeof(ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI), timerId, t);
        }

        private class ApprovalWorkflowWithLambdaAndApprovedTimedoutCheck : Workflow
        {
            public ApprovalWorkflowWithLambdaAndApprovedTimedoutCheck()
            {
                ScheduleLambda(ApproveExpense)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Approved").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck : Workflow
        {
            public ApprovalWorkflowWithLambdaAndRejectedTimedoutCheck()
            {
                ScheduleLambda(ApproveExpense)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithLambdaAndApprovedTimedoutCheckUsingAPI()
            {
                ScheduleLambda(ApproveExpense)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterLambda(ApproveExpense)
                    .When(l => l.ParentLambda().IsSignalTimedout("Approved"));
            }
        }

        private class ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithLambdaAndRejectedTimedoutCheckUsingAPI()
            {
                ScheduleLambda(ApproveExpense)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterLambda(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterLambda(ApproveExpense)
                    .When(l => l.ParentLambda().IsSignalTimedout("Rejected"));
            }
        }

        private class ApprovalWorkflowWithActivityAndApprovedTimedoutCheck : Workflow
        {
            public ApprovalWorkflowWithActivityAndApprovedTimedoutCheck()
            {
                ScheduleActivity(ApproveExpense, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterActivity(ApproveExpense, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterActivity(ApproveExpense, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterActivity(ApproveExpense, Version)
                    .When(_ => Signal("Approved").IsTimedout());
            }
        }

        private class ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi : Workflow
        {
            public ApprovalWorkflowWithActivityAndRejectedTimedoutCheckUsingApi()
            {
                ScheduleActivity(ApproveExpense, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterActivity(ApproveExpense, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterActivity(ApproveExpense, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterActivity(ApproveExpense, Version)
                    .When(l => l.ParentActivity().IsSignalTimedout("Rejected"));
            }
        }

        private class ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck : Workflow
        {
            public ApprovalWorkflowWithChildWorkflowAndApprovedTimeoutCheck()
            {
                ScheduleChildWorkflow(ApproveExpense, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterChildWorkflow(ApproveExpense, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterChildWorkflow(ApproveExpense, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterActivity(ApproveExpense, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalTimedout("Approved"));
            }
        }

        private class ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI : Workflow
        {
            public ApprovalWorkflowWithChildWorkflowAndRejectedTimeoutCheckUsingAPI()
            {
                ScheduleChildWorkflow(ApproveExpense, Version)
                    .OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterChildWorkflow(ApproveExpense, Version)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterChildWorkflow(ApproveExpense, Version)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterActivity(ApproveExpense, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalTimedout("Rejected"));
            }
        }


        private class ApproveWorkflowWithTimerAndApprovedTimeoutCheck : Workflow
        {
            public ApproveWorkflowWithTimerAndApprovedTimeoutCheck()
            {
                ScheduleTimer(ApproveExpense).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterTimer(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterTimer(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterTimer(ApproveExpense)
                    .When(_ => Signal("Approved").IsTimedout());
            }
        }

        private class ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI : Workflow
        {
            public ApproveWorkflowWithTimerAndRejectedTimeoutCheckUsingAPI()
            {
                ScheduleTimer(ApproveExpense).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForAnySignal("Approved", "Rejected").For(TimeSpan.FromHours(2)));

                ScheduleLambda(SubmitToAccount).AfterTimer(ApproveExpense)
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleLambda(SendBackToEmployee).AfterTimer(ApproveExpense)
                    .When(_ => Signal("Rejected").IsTriggered());

                ScheduleLambda(ApprovalTimedout).AfterTimer(ApproveExpense)
                    .When(l => l.ParentTimer().IsSignalTimedout("Rejected"));
            }
        }
    }
}