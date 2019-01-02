// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WaitForSignalTests
    {
        private HistoryEventsBuilder _builder;
        private EventGraphBuilder _graphBuilder;
        private ScheduleId _confirmEmailId;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _confirmEmailId = Identity.Lambda("ConfirmEmail").ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
        }

        [Test]
        public void Returns_the_decision_to_wait_for_a_signal()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddNewEvents(graph);
            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new[] { new WaitForSignalsDecision(_confirmEmailId, graph.First().EventId, "Confirmed") }));
        }

        [Test]
        public void Continue_the_execution_and_record_resumed_signal_when_signal_is_received()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, graph.First().EventId, new[] {"Confirmed"}, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, graph.First().EventId, "Confirmed"), 
            }));
        }

        [Test]
        public void Throws_exception_when_a_non_waiting_workflow_item_is_resumed()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new NonWaitingUserActivateWorkflow();
            Assert.Throws<SignalResumeException>(() => workflow.Decisions(_builder.Result()));
        }

        [Test]
        public void Throws_exception_when_a_waiting_workflow_item_is_resumed_with_not_expected_signal()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, graph.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflowWithUnexpectedSignal();
            Assert.Throws<SignalResumeException>(() => workflow.Decisions(_builder.Result()));
        }

        [Test]
        public void Signal_is_ignored_when_workflow_item_is_already_resumed()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, graph.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, graph.First().EventId, "Confirmed"));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Signal_is_ignored_when_no_workflow_item_is_waiting_for_it()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
              _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Different_To_Confirmed", ""));

            var workflow = new NonWaitingUserActivateWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.Empty);
        }

        [Test]
        public void Resume_the_longest_waiting_workflow_item_when_multiple_workflow_items_are_waiting_for_same_signal()
        {
            var l1 = Identity.Lambda("LambdaA1").ScheduleId();
            var l2 = Identity.Lambda("LambdaB1").ScheduleId();
            var w1 = _graphBuilder.LambdaCompletedEventGraph(l1, "input", "result");
            _builder.AddProcessedEvents(w1);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(l1, w1.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));

            var w2 = _graphBuilder.LambdaCompletedEventGraph(l2, "input", "result");
            _builder.AddProcessedEvents(w2);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(l2, w2.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));


            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new MultipleWorkflowItemsWaitingForSameSignal();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("LambdaA2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l1, w1.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Resume_two_parallel_waiting_workflow_items_when_two_signals_are_received()
        {
            var l1 = Identity.Lambda("LambdaA1").ScheduleId();
            var l2 = Identity.Lambda("LambdaB1").ScheduleId();
            var w1 = _graphBuilder.LambdaCompletedEventGraph(l1, "input", "result");
            _builder.AddProcessedEvents(w1);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(l1, w1.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));

            var w2 = _graphBuilder.LambdaCompletedEventGraph(l2, "input", "result");
            _builder.AddProcessedEvents(w2);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(l2, w2.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));


            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new MultipleWorkflowItemsWaitingForSameSignal();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("LambdaA2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l1, w1.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("LambdaB2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l2, w2.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Out_of_two_similar_signals_one_trigger_the_continuation_of_execution_and_other_one_is_ignored()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, graph.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, graph.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Continue_the_execution_and_record_resumed_signal_when_signal_is_received_along_with_wait_for_signal_decision()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(_confirmEmailId, graph.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, graph.First().EventId, "Confirmed")
            }));
        }

        [Test]
        public void Ignore_the_second_signal_when_two_similar_signals_are_received_along_with_wait_for_signal_decision()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddNewEvents(graph);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(_confirmEmailId, graph.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, graph.First().EventId, "Confirmed")
            }));
        }

        [Test]
        public void Continue_the_executions_of_both_branches_when_two_similar_signals_are_received_along_with_two_wait_for_signal_decisions()
        {
            var l1 = Identity.Lambda("LambdaA1").ScheduleId();
            var l2 = Identity.Lambda("LambdaB1").ScheduleId();
            var w1 = _graphBuilder.LambdaCompletedEventGraph(l1, "input", "result");
            _builder.AddNewEvents(w1);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var w2 = _graphBuilder.LambdaCompletedEventGraph(l2, "input", "result");
            _builder.AddNewEvents(w2);

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new MultipleWorkflowItemsWaitingForSameSignal();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(l1, w1.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("LambdaA2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l1, w1.First().EventId, "Confirmed"),

                new WaitForSignalsDecision(l2, w2.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("LambdaB2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l2, w2.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Continue_the_executions_of_both_branches_when_one_of_the_signal_is_received_along_with_wait_for_signal_decision()
        {
            var l1 = Identity.Lambda("LambdaA1").ScheduleId();
            var l2 = Identity.Lambda("LambdaB1").ScheduleId();
            var w1 = _graphBuilder.LambdaCompletedEventGraph(l1, "input", "result");
            _builder.AddProcessedEvents(w1);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(l1, w1.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("confirmed", ""));

            var w2 = _graphBuilder.LambdaCompletedEventGraph(l2, "input", "result");
            _builder.AddNewEvents(w2);

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new MultipleWorkflowItemsWaitingForSameSignal();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("LambdaA2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l1, w1.First().EventId, "Confirmed"),

                new WaitForSignalsDecision(l2, w2.First().EventId, "Confirmed"),
                new ScheduleLambdaDecision(Identity.Lambda("LambdaB2").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(l2, w2.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Continue_the_execution_when_waiting_item_has_the_history_of_waiting_for_same_signal()
        {
            var g1 = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(g1);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, g1.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, g1.First().EventId, "Confirmed"));

            var g2 = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(g2);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, g2.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EquivalentTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, g2.First().EventId, "Confirmed"),
            }));
        }

        [Test]
        public void Ignore_the_signal_when_waiting_workflow_item_has_already_been_signalled_multiple_times_in_past()
        {
            var g1 = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(g1);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, g1.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, g1.First().EventId, "Confirmed"));

            var g2 = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddProcessedEvents(g2);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_confirmEmailId, g2.First().EventId, new[] { "Confirmed" }, SignalWaitType.Any));
            _builder.AddProcessedEvents(_graphBuilder.WorkflowItemSignalledEvent(_confirmEmailId, g2.First().EventId, "Confirmed"));

            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.Empty);
        }

        [Test]
        public void Continue_the_execution_when_signal_received_along_with_composite_wait_for_signal_action()
        {
            var g1 = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddNewEvents(g1);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Confirmed", ""));

            var workflow = new UserActivateWorkflowWithCompositeAction();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(_confirmEmailId, g1.First().EventId,"Confirmed"),
                new RecordMarkerWorkflowDecision("Marker1", "details"),
                new ScheduleLambdaDecision(Identity.Lambda("ActivateUser").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_confirmEmailId, g1.First().EventId, "Confirmed"),
            }));
        }

        private class UserActivateWorkflow : Workflow
        {
            public UserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed"));
                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }
        }

        private class UserActivateWorkflowWithCompositeAction : Workflow
        {
            public UserActivateWorkflowWithCompositeAction()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed") + RecordMarker("Marker1", "details"));
                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }
        }

        private class UserActivateWorkflowWithUnexpectedSignal : Workflow
        {
            public UserActivateWorkflowWithUnexpectedSignal()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed"));

                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }

            [SignalEvent]
            public WorkflowAction Confirmed() => Lambda("ConfirmEmail").Resume("UnexpectedSignal");
        }

        private class NonWaitingUserActivateWorkflow : Workflow
        {
            public NonWaitingUserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail");
                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }

            [SignalEvent]
            public WorkflowAction Confirmed(string signalName) => Lambda("ConfirmEmail").Resume(signalName);
        }

        private class MultipleWorkflowItemsWaitingForSameSignal : Workflow
        {
            public MultipleWorkflowItemsWaitingForSameSignal()
            {
                ScheduleLambda("LambdaA1")
                    .OnCompletion(e => e.WaitForSignal("Confirmed"));
                ScheduleLambda("LambdaA2").AfterLambda("LambdaA1");

                ScheduleLambda("LambdaB1")
                    .OnCompletion(e => e.WaitForSignal("Confirmed"));
                ScheduleLambda("LambdaB2").AfterLambda("LambdaB1");

            }
        }
    }
}