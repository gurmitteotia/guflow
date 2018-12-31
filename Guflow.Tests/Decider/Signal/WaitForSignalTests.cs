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

        private class UserActivateWorkflow : Workflow
        {
            public UserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed"));

                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }
        }
    }
}