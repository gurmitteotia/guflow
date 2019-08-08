using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    public class WaitForSignalActionWithTimeoutTests
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
        public void Returns_the_timer_decision_and_decision_to_wait_for_a_signal()
        {
            var graph = _graphBuilder.LambdaCompletedEventGraph(_confirmEmailId, "input", "result");
            _builder.AddNewEvents(graph);
            var workflow = new UserActivateWorkflow();
            var decision = workflow.Decisions(_builder.Result());

            Assert.That(decision, Is.EqualTo(new WorkflowDecision[]
            {
                new WaitForSignalsDecision(new WaitForSignalData{ScheduleId = _confirmEmailId, TriggerEventId = graph.First().EventId}),
                new ScheduleTimerDecision(_confirmEmailId, TimeSpan.FromHours(2),true)
            }));
            var swfDecision = decision.First().SwfDecision();
            var data = swfDecision.RecordMarkerDecisionAttributes.Details.AsDynamic();
            Assert.That(data.SignalNames.ToObject<string[]>(), Is.EqualTo(new[] { "Confirmed" }));
            Assert.That((SignalWaitType)data.WaitType, Is.EqualTo(SignalWaitType.Any));
            Assert.That((SignalNextAction)data.NextAction, Is.EqualTo(SignalNextAction.Continue));
        }

        private class UserActivateWorkflow : Workflow
        {
            public UserActivateWorkflow()
            {
                ScheduleLambda("ConfirmEmail")
                    .OnCompletion(e => e.WaitForSignal("Confirmed").For(TimeSpan.FromHours(2)));
                ScheduleLambda("ActivateUser").AfterLambda("ConfirmEmail");
            }
        }
    }
}