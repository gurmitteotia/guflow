using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WaitForSignalActionWithTimeoutTests
    {
        private HistoryEventsBuilder _builder;
        private static EventGraphBuilder _graphBuilder;
        private ScheduleId _activateUserId;
        private ScheduleId _blockAccountId;
        private const string Version = "1.0";
        private const string ConfirmEmail = "ConfirmEmail";
        private const string WorkflowRunId = "id";
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _activateUserId = Identity.Lambda("ActivateUser").ScheduleId();
            _blockAccountId = Identity.Lambda("BlockAccount").ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddWorkflowRunId(WorkflowRunId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal_when_the_workflow_item_completed_event_is_processed_immediately
                    (Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow;
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(confirmEmailId, completedEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(confirmEmailId, completedEventId, TimeSpan.FromHours(2));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Reduce_signal_timer_timeout_by_an_hour_when_execution_of_wait_for_signal_workflow_action_is_delayed_by_one_hour(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-1);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();


            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(confirmEmailId, completedEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(confirmEmailId, completedEventId, TimeSpan.FromHours(1));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Returns_the_timer_decision_to_fire_immediately_and_decision_to_wait_for_a_signal_when_workflow_item_completed_event_is_processed_after_timeout(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var completedEventId = graph.First().EventId;

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertWaitForSignal(confirmEmailId, completedEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(confirmEmailId, completedEventId, TimeSpan.FromHours(0));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_is_ignored_when_it_come_after_wait_is_timedout_by_signal_timer(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completionDate = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completionDate);
            _builder.AddProcessedEvents(graph);
            var completedEventId = graph.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, completedEventId, completionDate, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(2), TimerType.SignalTimer, completedEventId);
            _builder.AddProcessedEvents(timerFiredGraph);
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalTimedoutEvent(confirmEmailId, completedEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", DateTime.UtcNow));
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Wait_for_signal_is_timedout_when_timer_is_fired_before_receiving_the_signal(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, signalTriggerEventId, completedStamp, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Count(), Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "input")));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, signalTriggerEventId, completedStamp, "Confirmed"));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1)));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(confirmEmailId, signalTriggerEventId, "Confirmed"));
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            _builder.AddNewEvents(timerFiredGraph);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions, Is.Empty);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_triggered_when_signal_and_activity_completed_events_comes_together_and_signal_come_before_timeout(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var currentTime = DateTime.UtcNow;
            var completedStamp = currentTime.AddHours(-1);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", currentTime);
            _builder.AddNewEvents(s);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);
            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            var triggerEventId = graph.First().EventId;
            Assert.That(decisions.Length, Is.EqualTo(4));
            decisions[0].AssertWaitForSignal(confirmEmailId, triggerEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[3], Is.EqualTo(new WorkflowItemSignalledDecision(confirmEmailId, triggerEventId, "Confirmed", s.EventId)));
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Return_timer_decision_and_continue_execution_with_signal_timedout_when_signal_and_activity_completed_events_comes_together_but_signal_comes_after_timeout(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-4);
            var graph = completedGraph(completedStamp);
            _builder.AddNewEvents(graph);
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "", DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            _builder.AddNewEvents(_graphBuilder.DecisionStartedEvent(DateTime.UtcNow));
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(4));

            var triggerEventId = graph.First().EventId;
            decisions[0].AssertWaitForSignal(confirmEmailId, triggerEventId, completedStamp, TimeSpan.FromHours(2), "Confirmed");
            decisions[1].AssertSignalTimer(confirmEmailId, triggerEventId, TimeSpan.FromHours(1));
            Assert.That(decisions[2], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
            decisions[3].AssertSignalTimedout(confirmEmailId, triggerEventId, new[] { "Confirmed" }, s.EventId);
        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, signalTriggerEventId, completeDateTime, "Confirmed"));
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_activateUserId, "")));
            Assert.That(decisions[1], Is.EqualTo(new WorkflowItemSignalledDecision(confirmEmailId, signalTriggerEventId, "Confirmed", s.EventId)));

        }

        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_is_ignored_when_it_is_received_after_signal_timer_and_signal_timer_and_signal_are_processed_together(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completeDateTime = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completeDateTime);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, signalTriggerEventId, completeDateTime, "Confirmed"));

            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);
            _builder.AddNewEvents(timerFiredGraph);

            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow.AddHours(-1));
            _builder.AddNewEvents(s);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            decisions[0].AssertSignalTimedout(confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, timerFiredGraph.First().EventId);
            Assert.That(decisions[1], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
        }


        [TestCaseSource(nameof(TestCaseData))]
        public void Signal_timer_is_ignored_when_it_is_fired_after_receiving_the_signal_and_signal_timer_and_signal_are_processed_together_also_signal_was_received_after_timedout(
            Type workflowType, ScheduleId confirmEmailId, CompletedEventGraph completedGraph)
        {
            var completedStamp = DateTime.UtcNow.AddHours(-2);
            var lg = completedGraph(completedStamp);
            _builder.AddProcessedEvents(lg);
            var signalTriggerEventId = lg.First().EventId;
            _builder.AddProcessedEvents(WaitForSignalEvent(confirmEmailId, signalTriggerEventId, completedStamp, "Confirmed"));
            var s = _graphBuilder.WorkflowSignaledEvent("Confirmed", "input", completedTime: DateTime.UtcNow);
            _builder.AddNewEvents(s);
            var timerFiredGraph = _graphBuilder.TimerFiredGraph(confirmEmailId, TimeSpan.FromHours(1), TimerType.SignalTimer, signalTriggerEventId);

            _builder.AddNewEvents(timerFiredGraph);
            var workflow = (Workflow)Activator.CreateInstance(workflowType);

            var decisions = workflow.Decisions(_builder.Result()).ToArray();

            Assert.That(decisions.Length, Is.EqualTo(2));
            Assert.That(decisions[0], Is.EqualTo(new ScheduleLambdaDecision(_blockAccountId, "")));
            decisions[1].AssertSignalTimedout(confirmEmailId, signalTriggerEventId, new[] { "Confirmed" }, s.EventId);
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
            return _graphBuilder.ChildWorkflowCompletedGraph(id,  "rid" ,"input", "res", completionStamp: completeDateTime)
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

            var lambdaId = Identity.Lambda(ConfirmEmail).ScheduleId();
            var l = new CompletedEventGraph(e => LambdaCompletedEventGraph(lambdaId, e));
            yield return new TestCaseData(typeof(UserActivateWorkflowWithLambda), lambdaId, l);
            yield return new TestCaseData(typeof(UserActivateWorkflowWithLambdaAndSignalAPI), lambdaId, l);

            var activityId = Identity.New(ConfirmEmail, Version).ScheduleId();
            var a = new CompletedEventGraph(e => ActivityCompletedEventGraph(activityId, e));
            yield return new TestCaseData(typeof(UserActivateWorkflowWithActivity), activityId, a);
            yield return new TestCaseData(typeof(UserActivateWorkflowWithActivityAndSignalAPI), activityId, a);

            var workflowId = Identity.New(ConfirmEmail, Version).ScheduleId(WorkflowRunId);
            var c = new CompletedEventGraph(e => ChildWorkflowCompletedEventGraph(workflowId, e));
            yield return new TestCaseData(typeof(UserActivateWorkflowWithChildWorkflow), workflowId, c);
            yield return new TestCaseData(typeof(UserActivateWorkflowWithChildWorkflowAndSignalAPI), workflowId, c);

            var timerId = Identity.Timer(ConfirmEmail).ScheduleId();
            var t = new CompletedEventGraph(e => TimerFiredEventGraph(timerId, e));
            yield return new TestCaseData(typeof(UserActivateWorkflowWithTimer), timerId, t);
            yield return new TestCaseData(typeof(UserActivateWorkflowWithTimerAndSignalAPI), timerId, t);
        }

        private class UserActivateWorkflowWithActivity : Workflow
        {
            public UserActivateWorkflowWithActivity()
            {
                ScheduleActivity(ConfirmEmail, Version)
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterActivity(ConfirmEmail, Version)
                    .When(_ => Signal("Confirmed").IsTriggered());

                ScheduleLambda("BlockAccount").AfterActivity(ConfirmEmail, Version)
                    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }

        private class UserActivateWorkflowWithActivityAndSignalAPI : Workflow
        {
            public UserActivateWorkflowWithActivityAndSignalAPI()
            {
                ScheduleActivity(ConfirmEmail, Version)
                   .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterActivity(ConfirmEmail, Version)
                    .When(l => l.ParentActivity().IsSignalled("Confirmed"));

                ScheduleLambda("BlockAccount").AfterActivity(ConfirmEmail, Version)
                    .When(l => l.ParentActivity().IsSignalTimedout("Confirmed"));
            }
        }

        private class UserActivateWorkflowWithLambda : Workflow
        {
            public UserActivateWorkflowWithLambda()
            {
                ScheduleLambda(ConfirmEmail)
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterLambda(ConfirmEmail)
                    .When(_ => Signal("Confirmed").IsTriggered());

                ScheduleLambda("BlockAccount").AfterLambda(ConfirmEmail)
                    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }

        private class UserActivateWorkflowWithLambdaAndSignalAPI : Workflow
        {
            public UserActivateWorkflowWithLambdaAndSignalAPI()
            {
                ScheduleLambda(ConfirmEmail)
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterLambda(ConfirmEmail)
                    .When(l => l.ParentLambda().IsSignalled("Confirmed"));

                ScheduleLambda("BlockAccount").AfterLambda(ConfirmEmail)
                    .When(l => l.ParentLambda().IsSignalTimedout("Confirmed"));
            }
        }

        private class UserActivateWorkflowWithChildWorkflow : Workflow
        {
            public UserActivateWorkflowWithChildWorkflow()
            {
                ScheduleChildWorkflow(ConfirmEmail, Version)
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterChildWorkflow(ConfirmEmail, Version)
                    .When(_ => Signal("Confirmed").IsTriggered());

                ScheduleLambda("BlockAccount").AfterChildWorkflow(ConfirmEmail, Version)
                    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }

        private class UserActivateWorkflowWithChildWorkflowAndSignalAPI : Workflow
        {
            public UserActivateWorkflowWithChildWorkflowAndSignalAPI()
            {
                ScheduleChildWorkflow(ConfirmEmail, Version)
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterChildWorkflow(ConfirmEmail, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalled("Confirmed"));

                ScheduleLambda("BlockAccount").AfterChildWorkflow(ConfirmEmail, Version)
                    .When(l => l.ParentChildWorkflow().IsSignalTimedout("Confirmed"));
            }
        }


        private class UserActivateWorkflowWithTimer : Workflow
        {
            public UserActivateWorkflowWithTimer()
            {
                ScheduleTimer(ConfirmEmail).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterTimer(ConfirmEmail)
                    .When(_ => Signal("Confirmed").IsTriggered());

                ScheduleLambda("BlockAccount").AfterTimer(ConfirmEmail)
                    .When(_ => Signal("Confirmed").IsTimedout());
            }
        }

        private class UserActivateWorkflowWithTimerAndSignalAPI : Workflow
        {
            public UserActivateWorkflowWithTimerAndSignalAPI()
            {
                ScheduleTimer(ConfirmEmail).FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));

                ScheduleLambda("ActivateUser").AfterTimer(ConfirmEmail)
                    .When(l => l.ParentTimer().IsSignalled("Confirmed"));

                ScheduleLambda("BlockAccount").AfterTimer(ConfirmEmail)
                    .When(l => l.ParentTimer().IsSignalTimedout("Confirmed"));
            }
        }

    }
}