// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
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
    public class TimerItemTests
    {
        private readonly Identity _timerIdentity = Identity.Timer("timerName");
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Mock<IWorkflow> _workflow;
        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_graphBuilder.WorkflowStartedEvent());
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(_builder.Result());
        }

        [Test]
        public void By_default_schedule_timer_to_fire_immediately()
        {
            var timerItem = TimerItem.New(_timerIdentity, _workflow.Object);

            var decision = timerItem.ScheduleDecisions();

            Assert.That(decision,Is.EqualTo(new []{ScheduleTimerDecision.WorkflowItem(_timerIdentity.ScheduleId(), new TimeSpan())}));
        }

        [Test]
        public void Can_be_configured_to_schedule_timer_to_fire_after_timeout()
        {
            var timerItem = TimerItem.New(_timerIdentity, _workflow.Object);
            timerItem.FireAfter(TimeSpan.FromSeconds(3));
            var decision = timerItem.ScheduleDecisions();

            Assert.That(decision, Is.EqualTo(new []{ ScheduleTimerDecision.WorkflowItem(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(3))}));
        }

        [Test]
        public void Can_be_configured_to_schedule_timer_to_fire_after_timeout_using_lambda()
        {
            var timerItem = TimerItem.New(_timerIdentity, _workflow.Object);
            timerItem.FireAfter(_=>TimeSpan.FromSeconds(4));
            var decision = timerItem.ScheduleDecisions();

            Assert.That(decision, Is.EqualTo(new[] { ScheduleTimerDecision.WorkflowItem(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(4)) }));
        }

        [Test]
        public void Fire_after_lambda_handler_override_the_fire_after_timeout()
        {
            var timerItem = TimerItem.New(_timerIdentity, _workflow.Object);
            timerItem.FireAfter(_ => TimeSpan.FromSeconds(3)).FireAfter(TimeSpan.FromSeconds(4));
            var decision = timerItem.ScheduleDecisions();

            Assert.That(decision, Is.EqualTo(new[] { ScheduleTimerDecision.WorkflowItem(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(3)) }));
        }
        [Test]
        public void Return_empty_when_when_condition_is_evaluated_to_false()
        {
            var timerItem = TimerItem.New(_timerIdentity, Mock.Of<IWorkflow>());
            timerItem.When(t => false);

            var decisions = timerItem.ScheduleDecisions();

            Assert.That(decisions,Is.Empty);
        }

        [Test]
        public void Last_event_can_be_timer_started_event()
        {
            var eventGraph = _graphBuilder.TimerStartedGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new TimerStartedEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_is_cached()
        {
            var eventGraph = _graphBuilder.TimerStartedGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent(true);

            Assert.True(ReferenceEquals(latestEvent, timerItem.LastEvent(true)));
        }

        [Test]
        public void Last_event_can_be_timer_fired_event()
        {
            var eventGraph =_graphBuilder.TimerFiredGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new TimerFiredEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_filter_out_timer_start_failed_event()
        {
            var started = _graphBuilder.TimerStartedGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(1));
            var failed =_graphBuilder.TimerStartFailedGraph(_timerIdentity.ScheduleId(), "cause");
            var timerItem = CreateTimerItemFor(failed.Concat(started));

            var latestEvent = timerItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new TimerStartedEvent(started.First(), started)));
        }

        [Test]
        public void Last_event_can_be_timer_cancelled_event()
        {
            var eventGraph = _graphBuilder.TimerCancelledGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new TimerCancelledEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_filters_out_timer_cancellation_failed_event()
        {
            var started = _graphBuilder.TimerStartedGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(1));
            var failed =_graphBuilder.TimerCancellationFailedGraph(_timerIdentity.ScheduleId(), "cause");
            var timerItem = CreateTimerItemFor(failed.Concat(started));

            var latestEvent = timerItem.LastEvent(true);

            Assert.That(latestEvent, Is.EqualTo(new TimerStartedEvent(started.First(), started)));
        }

        [Test]
        public void All_events_can_be_timer_cancellation_failed_event_and_timer_started_event()
        {
            var started = _graphBuilder.TimerStartedGraph(_timerIdentity.ScheduleId(), TimeSpan.Zero).ToArray();
            var failed = _graphBuilder.TimerCancellationFailedGraph(_timerIdentity.ScheduleId(), "cause").ToArray();
            var timerItem = CreateTimerItemFor(failed.Concat(started));

            var allEvents = timerItem.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerCancellationFailedEvent(failed.First()),
                new TimerStartedEvent(started.First(), started)
            }));
        }

        [Test]
        public void All_events_can_return_timer_fired_events()
        {
            var eventGraph = _graphBuilder.TimerFiredGraph(_timerIdentity.ScheduleId(), TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.AllEvents(true);

            Assert.That(latestEvent, Is.EqualTo(new[]{new TimerFiredEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void Invalid_arguments_test()
        {
            var timerItem = (IFluentTimerItem) TimerItem.New(_timerIdentity, null);

            Assert.Throws<ArgumentNullException>(() => timerItem.OnCancellationFailed(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnCancel(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnFired(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnStartFailed(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.When(null));

            Assert.Throws<ArgumentException>(() => timerItem.AfterActivity(null,"1.0"));
            Assert.Throws<ArgumentException>(() => timerItem.AfterActivity("1.0", null));
            Assert.Throws<ArgumentException>(() => timerItem.AfterTimer(null));
            Assert.Throws<ArgumentException>(() => timerItem.AfterLambda(null));
            Assert.Throws<ArgumentException>(() => timerItem.AfterChildWorkflow(null, "1.0"));
            Assert.Throws<ArgumentException>(() => timerItem.AfterChildWorkflow("v", null));

        }

        private TimerItem CreateTimerItemFor(IEnumerable<HistoryEvent> eventGraph)
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);
            var workflow = new Mock<IWorkflow>();
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(workflowHistoryEvents);
            return TimerItem.New(_timerIdentity, workflow.Object);
        }
    }
}