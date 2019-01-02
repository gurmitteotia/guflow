// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WaitForAnySignalActionTests
    {
        private HistoryEventsBuilder _builder;
        private EventGraphBuilder _graphBuilder;
        private ScheduleId _sendForApprovalId;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _sendForApprovalId = Identity.Lambda("SendForApproval").ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
        }

        [Test]
        public void Returns_wait_for_signals_decision()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddNewEvents(graph);
            var workflow = new ExpenseWorkflowToSendEmail();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new WaitForSignalsDecision(_sendForApprovalId, graph.First().EventId ) }));
            var attr = decisions.First().SwfDecision().RecordMarkerDecisionAttributes;
            var data = attr.Details.AsDynamic();
            Assert.That(data.SignalNames.ToObject<string[]>(), Is.EqualTo(new[] { "Approved", "Rejected" }));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(SignalWaitType.Any));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(SignalNextAction.Continue));
        }

        [Test]
        public void Continue_the_execution_when_any_of_waiting_signal_is_received()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_sendForApprovalId, graph.First().EventId, new[] { "Approved", "Rejected" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Approved", ""));


            var workflow = new ExpenseWorkflowToSendEmail();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("SendEmail").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_sendForApprovalId, graph.First().EventId, "Approved") 
            }));
        }

        [Test]
        public void Second_signal_is_ignored_when_both_signals_are_received()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_sendForApprovalId, graph.First().EventId, new[] { "Approved", "Rejected" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Approved", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Rejected", ""));


            var workflow = new ExpenseWorkflowToSendEmail();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("SendEmail").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_sendForApprovalId, graph.First().EventId, "Approved")
            }));
        }


        [Test]
        public void First_signal_is_received_signal_and_can_be_used_in_scheduling_the_specific_children()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_sendForApprovalId, graph.First().EventId, new[] { "Approved", "Rejected" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Approved", ""));

            var workflow = new ExpenseWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("SendToAccount").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_sendForApprovalId, graph.First().EventId, "Approved")
            }));
        }


        [Test]
        public void Second_signal_is_received_signal_and_can_be_used_in_scheduling_the_specific_children()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_sendForApprovalId, graph.First().EventId, new[] { "Approved", "Rejected" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Rejected", ""));

            var workflow = new ExpenseWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("SendRejectEmail").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_sendForApprovalId, graph.First().EventId, "Rejected")
            }));
        }

        [Test]
        public void Second_signal_is_ignored_and_it_is_not_used_in_signalling_the_waiting_item()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_sendForApprovalId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_sendForApprovalId, graph.First().EventId, new[] { "Approved", "Rejected" }, SignalWaitType.Any));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Approved", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Rejected", ""));

            var workflow = new ExpenseWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new ScheduleLambdaDecision(Identity.Lambda("SendToAccount").ScheduleId(), "input"),
                new WorkflowItemSignalledDecision(_sendForApprovalId, graph.First().EventId, "Approved")
            }));
        }

        private class ExpenseWorkflowToSendEmail : Workflow
        {
            public ExpenseWorkflowToSendEmail()
            {
                ScheduleLambda("SendForApproval").OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected"));

                ScheduleLambda("SendEmail").AfterLambda("SendForApproval");
            }
        }

        private class ExpenseWorkflow : Workflow
        {
            public ExpenseWorkflow()
            {
                ScheduleLambda("SendForApproval").OnCompletion(e => e.WaitForAnySignal("Approved", "Rejected"));

                ScheduleLambda("SendToAccount").AfterLambda("SendForApproval")
                    .When(l => l.ParentLambda().IsSignalled("Approved"));

                ScheduleLambda("SendRejectEmail").AfterLambda("SendForApproval")
                    .When(l => l.ParentLambda().IsSignalled("Rejected"));
            }
        }
    }
}