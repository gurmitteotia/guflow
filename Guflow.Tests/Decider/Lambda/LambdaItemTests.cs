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
        private ScheduleId _scheduleId;
        private const string LambdaName = "lambda";
        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph()));
            _lambdaIdentity = Identity.Lambda(LambdaName);
            _scheduleId = _lambdaIdentity.ScheduleId();
        }
        [Test]
        public void By_default_lambda_function_is_scheduled_with_workflow_input()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "\"actvity\"";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);

            var decisions = lambdaItem.ScheduleDecisions();
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
            var decisions = lambdaItem.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo("\"CustomInput\""));
        }

        [Test]
        public void Enclose_the_lambda_input_string_if_it_is_already_not()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);

            var decisions = lambdaItem.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo("\""+workflowInput+"\""));
        }

        [Test]
        public void Does_not_put_json_string_in_quotes()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "{\"Id\":\"10\"}";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);

            var decisions = lambdaItem.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo(workflowInput));
        }


        [Test]
        public void Does_not_put_non_string_in_quotes()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(_lambdaIdentity, workflow.Object);
            lambdaItem.WithInput(i => 10);
            var decisions = lambdaItem.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo("10"));
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
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterChildWorkflow(null,"ver"));
            Assert.Throws<ArgumentException>(() => lambdaItem.AfterChildWorkflow("name", null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.When(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.When(null,_=>WorkflowAction.Empty));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.When(_=>true, null));

        }

        [Test]
        public void Cancel_decision_is_empty()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, Mock.Of<IWorkflow>());
            Assert.That(lambdaItem.CancelDecisions(), Is.Empty);
        }

        [Test]
        public void By_default_timeout_of_lamdba_fuction_is_null()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);

            var swfDecision = lambdaItem.ScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.Null);
        }

        [Test]
        public void Time_out_scheduling_lambda_function_can_be_customized()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);
            lambdaItem.WithTimeout(i => TimeSpan.FromSeconds(10));
            var swfDecision = lambdaItem.ScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.EqualTo("10"));
        }

        [Test]
        public void Reschedule_decision_is_a_timer_decision_for_lambda_item()
        {
            var lambdaItem = new LambdaItem(_lambdaIdentity, _workflow.Object);
            var decision = lambdaItem.RescheduleDecisions(TimeSpan.FromSeconds(10));
            Assert.That(decision, Is.EqualTo(new []{ScheduleTimerDecision.RescheduleTimer(_lambdaIdentity.ScheduleId(), TimeSpan.FromSeconds(10))}));
        }

        [Test]
        public void All_events_can_return_lamdba_completed_event()
        {
            var eventGraph =
                _builder.LambdaCompletedEventGraph(_scheduleId, "input", "result", TimeSpan.FromSeconds(2));
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new []{new LambdaCompletedEvent(eventGraph.First(), eventGraph)}));
        }

        [Test]
        public void All_events_can_return_lamdba_failed_event()
        {
            var eventGraph =
                _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_timedout_event()
        {
            var eventGraph = _builder.LamdbaTimedoutEventGraph(_scheduleId, "input", "reason");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaTimedoutEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_started_event()
        {
            var eventGraph = _builder.LambdaStartedEventGraph(_scheduleId, "input");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaStartedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_start_failed_event()
        {
            var eventGraph = _builder.LambdaStartFailedEventGraph(_scheduleId, "input", "reason", "msg");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaStartFailedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_scheduled_event()
        {
            var eventGraph = _builder.LambdaScheduledEventGraph(_scheduleId, "input");
            var lamdbaItem = CreateLambdaItem(new[]{eventGraph});

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaScheduledEvent(eventGraph) }));
        }

        [Test]
        public void All_events_can_return_lamdba_scheduling_failed_event()
        {
            var eventGraph = _builder.LambdaSchedulingFailedEventGraph(_scheduleId, "reason");
            var lamdbaItem = CreateLambdaItem(new[] { eventGraph });

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaSchedulingFailedEvent(eventGraph) }));
        }

        [Test]
        public void All_events_does_not_return_events_for_other_lambda_item()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph(_scheduleId, "input", "result", TimeSpan.FromSeconds(1));
            var otherLambdaEvent = _builder.LambdaCompletedEventGraph(Identity.Lambda("other").ScheduleId(), "input", "result", TimeSpan.FromSeconds(1));
            var lamdbaItem = CreateLambdaItem(eventGraph.Concat(otherLambdaEvent));

            var allEvents = lamdbaItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new[] { new LambdaCompletedEvent(eventGraph.First(), eventGraph) }));
        }

        [Test]
        public void All_events_can_return_multiple_independent_events_for_same_lambda_event()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var startedEventGraph = _builder.LambdaStartedEventGraph(_scheduleId, "input");
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lambdaEvents = lamdbaItem.AllEvents(true);

            Assert.That(lambdaEvents, Is.EqualTo(new LambdaEvent[]
            {
                new LambdaStartedEvent(startedEventGraph.First(), allEvents),
                new LambdaFailedEvent(failedEventGraph.First(), allEvents), 
            }));
        }

        [Test]
        public void All_events_can_return_reschedule_timer_events()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity.ScheduleId(), TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lambdaEvents = lamdbaItem.AllEvents(true);

            Assert.That(lambdaEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerStartedEvent(startedEventGraph.First(), allEvents), 
                new LambdaFailedEvent(failedEventGraph.First(), allEvents),
            }));
        }

        [Test]
        public void All_events_by_default_filters_out_reschedule_timer_events()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity.ScheduleId(), TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lambdaEvents = lamdbaItem.AllEvents();

            Assert.That(lambdaEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new LambdaFailedEvent(failedEventGraph.First(), allEvents),
            }));
        }

        [Test]
        public void Last_event_returns_latest_event_of_lambda_item()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var completedEventGraph = _builder.LambdaCompletedEventGraph(_scheduleId, "input", "result");
            var allEvents = completedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lastEvent = lamdbaItem.LastEvent(true);

            Assert.That(lastEvent, Is.EqualTo(new LambdaCompletedEvent(completedEventGraph.First(), allEvents)));
        }

        [Test]
        public void Last_event_can_return_event_for_rescheduled_timer()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity.ScheduleId(), TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lastEvent = lamdbaItem.LastEvent(true);

            Assert.That(lastEvent, Is.EqualTo(new TimerStartedEvent(startedEventGraph.First(), allEvents)));
        }

        [Test]
        public void Last_event_by_default_filters_out_rescheduled_timer()
        {
            var failedEventGraph = _builder.LambdaFailedEventGraph(_scheduleId, "input", "reason", "details");
            var startedEventGraph = _builder.TimerStartedGraph(_lambdaIdentity.ScheduleId(), TimeSpan.FromSeconds(1), true);
            var allEvents = startedEventGraph.Concat(failedEventGraph);
            var lamdbaItem = CreateLambdaItem(allEvents);

            var lastEvent = lamdbaItem.LastEvent();

            Assert.That(lastEvent, Is.EqualTo(new LambdaFailedEvent(failedEventGraph.First(), failedEventGraph)));
        }

        [Test]
        public void Last_event_filters_outs_lambda_scheduling_failed_event()
        {
            var started = _builder.LambdaStartedEventGraph(_scheduleId, "input", TimeSpan.FromSeconds(1));
            var failed =  new[]{_builder.LambdaSchedulingFailedEventGraph(_scheduleId, "reason")};
            var lamdbaItem = CreateLambdaItem(failed.Concat(started));

            var lastEvent = lamdbaItem.LastEvent();

            Assert.That(lastEvent, Is.EqualTo(new LambdaStartedEvent(started.First(), started)));
        }

        [Test]
        public void Last_event_is_cached()
        {
            var eventGraph = _builder.LambdaCompletedEventGraph(_scheduleId, "input", "result");
            var lamdbaItem = CreateLambdaItem(eventGraph);

            Assert.IsTrue(ReferenceEquals(lamdbaItem.LastEvent(true), lamdbaItem.LastEvent(true)));
        }
        private LambdaItem CreateLambdaItem(IEnumerable<HistoryEvent> allEvents)
        {
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(allEvents));
            return new LambdaItem(_lambdaIdentity, _workflow.Object);
        }
    }
}