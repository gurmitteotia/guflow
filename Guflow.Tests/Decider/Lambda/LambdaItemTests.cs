// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LambdaItemTests
    {
        private EventGraphBuilder _builder;
        private Mock<IWorkflow> _workflow;
        private Identity _lambdaIdentity;
        private const string LambdaName = "lambda";
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph()));
            _lambdaIdentity = Identity.Lambda(LambdaName);
        }
        [Test]
        public void By_default_lambda_function_is_scheduled_with_workflow_input()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);

            var decisions = lambdaItem.GetScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo(workflowInput));
        }

        [Test]
        public void Input_of_lambda_function_can_be_customized()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);
            lambdaItem.WithInput(i => "CustomInput");
            var decisions = lambdaItem.GetScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo("CustomInput"));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, Mock.Of<IWorkflow>());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.WithInput(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.WithTimeout(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnTimedout(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnSchedulingFailed(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnStartFailed(null));
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterActivity(null, "1.0"));
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterActivity("name", null));
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterTimer(null));
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterLambda(null));
        }

        [Test]
        public void Cancel_decision_is_empty()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, Mock.Of<IWorkflow>());
            Assert.That(lambdaItem.GetCancelDecision(), Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void By_default_timeout_of_lamdba_fuction_is_null()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);

            var swfDecision = lambdaItem.GetScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.Null);
        }

        [Test]
        public void Time_out_scheduling_lambda_function_can_be_customized()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);
            lambdaItem.WithTimeout(i => TimeSpan.FromSeconds(10));
            var swfDecision = lambdaItem.GetScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.EqualTo("10"));
        }

        [Test]
        public void Reschedule_decision_is_a_timer_decision_for_lambda_item()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);
            var decision = lambdaItem.GetRescheduleDecisions(TimeSpan.FromSeconds(10));
            Assert.That(decision, Is.EqualTo(new []{new ScheduleTimerDecision(_lambdaIdentity, TimeSpan.FromSeconds(10), true)}));
        }

        [Test]
        public void All_events_can_return_lamdba_completed_event()
        {
            var eventGraph =
                _builder.LambdaCompletedEventGraph(_lambdaIdentity, "input", "result", "con", TimeSpan.FromSeconds(2));
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new []{new LambdaCompletedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_lamdba_failed_event()
        {
            var eventGraph =
                _builder.LambdaFailedEventGraph(_lambdaIdentity, "input", "reason", "details" ,"con");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_timedout_event()
        {
            var eventGraph = _builder.LamdbaTimedoutEventGraph(_lambdaIdentity, "input", "reason", "details");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaTimedoutEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_started_event()
        {
            var eventGraph = _builder.LambdaStartedEventGraph(_lambdaIdentity, "input", "reason");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaStartedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_start_failed_event()
        {
            var eventGraph = _builder.LambdaStartFailedEventGraph(_lambdaIdentity, "input", "reason", "msg", "ctl");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaStartFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_scheduled_event()
        {
            var eventGraph = _builder.LambdaScheduledEventGraph(_lambdaIdentity, "input", "ctl");
            var lamdbaItem = CreateLambdaItem(new[]{eventGraph});

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaScheduledEvent(eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_scheduling_failed_event()
        {
            var eventGraph = _builder.LambdaSchedulingFailedEventGraph(_lambdaIdentity, "reason");
            var lamdbaItem = CreateLambdaItem(new[] { eventGraph });

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaSchedulingFailedEvent(eventGraph) }));
        }

        [Test]
        public void All_events_does_not_return_events_for_other_lambda_item()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph(_lambdaIdentity, "input", "result", "control", TimeSpan.FromSeconds(1));
            var otherLambdaEvent = _builder.LambdaCompletedEventGraph(Identity.Lambda("other"), "input", "result", "control", TimeSpan.FromSeconds(1));
            var lamdbaItem = CreateLambdaItem(eventGraph.Concat(otherLambdaEvent));

            var allEvents = lamdbaItem.AllEvents;

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaCompletedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_multiple_independent_events_for_same_lambda_event()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_lambdaIdentity, "input", "reason", "details", "control");
            var startedEventGraph = _builder.LambdaStartedEventGraph(_lambdaIdentity, "input", "control");
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lambdaEvents = lamdbaItem.AllEvents;

            Assert.That(lambdaEvents, Is.EqualTo(new LambdaEvent[]
            {
                new LambdaStartedEvent(startedEventGraph.First(), allEvents),
                new LambdaFailedEvent(failedEventGraph.First(), allEvents), 
            }));
        }

        [Test]
        public void All_events_can_return_reschedule_timer_events()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_lambdaIdentity, "input", "reason", "details", "control");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity, TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lambdaEvents = lamdbaItem.AllEvents;

            Assert.That(lambdaEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerStartedEvent(startedEventGraph.First(), allEvents), 
                new LambdaFailedEvent(failedEventGraph.First(), allEvents),
            }));
        }

        [Test]
        public void Last_event_returns_latest_event_of_lambda_item()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_lambdaIdentity, "input", "reason", "details", "control");
            var completedEventGraph = _builder.LambdaCompletedEventGraph(_lambdaIdentity, "input", "result", "control");
            var allEvents = completedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lastEvent = lamdbaItem.LastEvent;

            Assert.That(lastEvent, Is.EqualTo(new LambdaCompletedEvent(completedEventGraph.First(), allEvents)));
        }

        [Test]
        public void Last_event_can_return_event_for_rescheduled_timer()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_lambdaIdentity, "input", "reason", "details", "control");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity, TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lastEvent = lamdbaItem.LastEvent;

            Assert.That(lastEvent, Is.EqualTo(new TimerStartedEvent(startedEventGraph.First(), allEvents)));
        }

        [Test]
        public void Last_event_is_cached()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph(_lambdaIdentity, "input", "result", "control");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            Assert.IsTrue(ReferenceEquals(lamdbaItem.LastEvent, lamdbaItem.LastEvent));
        }
        private LambdaItem CreateLambdaItem(IEnumerable<HistoryEvent> allEvents)
        {
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(allEvents));
            return new LambdaItem(_lambdaIdentity, _workflow.Object);
        }
    }
}