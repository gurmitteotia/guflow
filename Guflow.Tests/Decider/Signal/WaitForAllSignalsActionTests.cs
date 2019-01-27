// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WaitForAllSignalsActionTests
    {
        private HistoryEventsBuilder _builder;
        private EventGraphBuilder _graphBuilder;
        private ScheduleId _promoteId;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _promoteId = Identity.Lambda("PromoteEmployee").ScheduleId();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
        }

        [Test]
        public void Returns_wait_all_for_signals_decision()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddNewEvents(graph);
            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[]
            {
                new WaitForSignalsDecision(new WaitForSignalData(){ScheduleId = _promoteId, TriggerEventId = graph.First().EventId})
            }));
            var attr = decisions.First().SwfDecision().RecordMarkerDecisionAttributes;
            var data = attr.Details.AsDynamic();
            Assert.That(data.SignalNames.ToObject<string[]>(), Is.EqualTo(new[] { "ManagerApproved", "HRApproved" }));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(SignalWaitType.All));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(SignalNextAction.Continue));
        }

        [Test]
        public void Continue_the_execution_to_children_when_all_signals_are_received()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_promoteId, graph.First().EventId,
                new[] {"ManagerApproved", "HRApproved"}, SignalWaitType.All));
            var s1 = _graphBuilder.WorkflowSignaledEvent("Managerapproved", "");
            _builder.AddNewEvents(s1);
            var s2 = _graphBuilder.WorkflowSignaledEvent("HRapproved", "");
            _builder.AddNewEvents(s2);

            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
                new ScheduleLambdaDecision(Identity.Lambda("EmployeePromoted").ScheduleId(),""),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"HRApproved", s2.EventId),
            }));
        }

        [Test]
        public void Ignore_duplicate_signals()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_promoteId, graph.First().EventId,
                new[] { "ManagerApproved", "HRApproved" }, SignalWaitType.All));
            var s1 = _graphBuilder.WorkflowSignaledEvent("Managerapproved", "");
            _builder.AddNewEvents(s1);
            var s2 = _graphBuilder.WorkflowSignaledEvent("HRapproved", "");
            _builder.AddNewEvents(s2);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Managerapproved", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRapproved", ""));

            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
                new ScheduleLambdaDecision(Identity.Lambda("EmployeePromoted").ScheduleId(),""),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"HRApproved", s2.EventId),
            }));
        }

        [Test]
        public void Does_not_continue_when_only_one_of_the_signal_is_received()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_promoteId, graph.First().EventId,
                new[] { "ManagerApproved", "HRApproved" }, SignalWaitType.All));
            var s1 = _graphBuilder.WorkflowSignaledEvent("ManagerApproved", "");
            _builder.AddNewEvents(s1);
   
            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new []
            {
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
            }));
        }

        [Test]
        public void Continue_the_execution_to_children_when_all_signals_are_received_immediately_when_workflow_is_about_to_wait_for_signals()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddNewEvents(graph);
              var s1 = _graphBuilder.WorkflowSignaledEvent("Managerapproved", "");
            _builder.AddNewEvents(s1);
            var s2 = _graphBuilder.WorkflowSignaledEvent("HRapproved", "");
            _builder.AddNewEvents(s2);

            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(new WaitForSignalData(){ScheduleId = _promoteId, TriggerEventId = graph.First().EventId}),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
                new ScheduleLambdaDecision(Identity.Lambda("EmployeePromoted").ScheduleId(),""),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"HRApproved", s2.EventId),
            }));
        }


        [Test]
        public void Ignore_the_extra_signals_received_received_when_workflow_has_executed_the_wait_for_all_signals_action()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddNewEvents(graph);
            var s1 = _graphBuilder.WorkflowSignaledEvent("Managerapproved", "");
            _builder.AddNewEvents(s1);
            var s2 = _graphBuilder.WorkflowSignaledEvent("HRapproved", "");
            _builder.AddNewEvents(s2);
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("Managerapproved", ""));
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("HRapproved", ""));
            var workflow = new PromoteWorkflow();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(new WaitForSignalData(){ScheduleId = _promoteId, TriggerEventId = graph.First().EventId}),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
                new ScheduleLambdaDecision(Identity.Lambda("EmployeePromoted").ScheduleId(),""),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"HRApproved", s2.EventId),
            }));
        }

        [Test]
        public void Waiting_of_signaled_can_be_queried_in_scheduling_the_children()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_promoteId, "input", "result");
            _builder.AddProcessedEvents(graph);
            _builder.AddProcessedEvents(_graphBuilder.WaitForSignalEvent(_promoteId, graph.First().EventId,
                new[] { "ManagerApproved", "HRApproved" }, SignalWaitType.All));
            var s1 = _graphBuilder.WorkflowSignaledEvent("Managerapproved", "");
            _builder.AddNewEvents(s1);
            var s2 = _graphBuilder.WorkflowSignaledEvent("HRapproved", "");
            _builder.AddNewEvents(s2);

            var workflow = new PromoteWorkflowWithSignalQuery();
            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new WorkflowDecision[]
            {
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"ManagerApproved", s1.EventId),
                new ScheduleLambdaDecision(Identity.Lambda("EmployeePromoted").ScheduleId(),""),
                new WorkflowItemSignalledDecision(_promoteId, graph.First().EventId,"HRApproved", s2.EventId),
            }));
        }


        private class PromoteWorkflow : Workflow
        {
            public PromoteWorkflow()
            {
                ScheduleLambda("PromoteEmployee")
                    .OnCompletion(e => e.WaitForAllSignals("ManagerApproved", "HRApproved"));

                ScheduleLambda("EmployeePromoted").AfterLambda("PromoteEmployee");
                ScheduleLambda("SendEmail").AfterLambda("EmployeePromoted");
            }
        }

        private class PromoteWorkflowWithSignalQuery : Workflow
        {
            public PromoteWorkflowWithSignalQuery()
            {
                ScheduleLambda("PromoteEmployee")
                    .OnCompletion(e => e.WaitForAllSignals("ManagerApproved", "HRApproved"));

                ScheduleLambda("EmployeePromoted").AfterLambda("PromoteEmployee")
                    .When(l => l.ParentLambda().IsSignalled("ManagerApproved") && l.ParentLambda().IsSignalled("HRApproved"));

                ScheduleLambda("SendEmail").AfterLambda("EmployeePromoted")
                    .When(l => l.ParentLambda().IsSignalled("ManagerApproved") && l.ParentLambda().IsSignalled("HRApproved"));
            }
        }
    }
}