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
        private EventGraphBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new EventGraphBuilder();
        }

        [Test]
        public void By_default_schedule_timer_to_fire_immediately()
        {
            var timerItem = TimerItem.New(_timerIdentity,null);

            var decision = timerItem.GetScheduleDecisions();

            Assert.That(decision,Is.EqualTo(new []{new ScheduleTimerDecision(_timerIdentity,new TimeSpan())}));
        }

        [Test]
        public void Can_be_configured_to_schedule_timer_to_fire_after_timeout()
        {
            var timerItem = TimerItem.New(_timerIdentity, null);
            timerItem.FireAfter(TimeSpan.FromSeconds(3));
            var decision = timerItem.GetScheduleDecisions();

            Assert.That(decision, Is.EqualTo(new []{new ScheduleTimerDecision(_timerIdentity, TimeSpan.FromSeconds(3))}));
        }

        [Test]
        public void Return_empty_when_when_condition_is_evaluated_to_false()
        {
            var timerItem = TimerItem.New(_timerIdentity, Mock.Of<IWorkflow>());
            timerItem.When(t => false);

            var decisions = timerItem.GetScheduleDecisions();

            Assert.That(decisions,Is.Empty);
        }

        [Test]
        public void Last_event_can_be_timer_started_event()
        {
            var eventGraph = _builder.TimerStartedGraph(_timerIdentity, TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new TimerStartedEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_is_cached()
        {
            var eventGraph = _builder.TimerStartedGraph(_timerIdentity, TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.True(ReferenceEquals(latestEvent, timerItem.LastEvent));
        }

        [Test]
        public void Last_event_can_be_timer_fired_event()
        {
            var eventGraph =_builder.TimerFiredGraph(_timerIdentity, TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new TimerFiredEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_can_be_timer_start_failed_event()
        {
            var eventGraph =_builder.TimerStartFailedGraph(_timerIdentity, "cause");
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new TimerStartFailedEvent(eventGraph.First())));
        }

        [Test]
        public void Last_event_can_be_timer_cancelled_event()
        {
            var eventGraph = _builder.TimerCancelledGraph(_timerIdentity, TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new TimerCancelledEvent(eventGraph.First(),eventGraph)));
        }

        [Test]
        public void Last_event_can_be_timer_cancellation_failed_event()
        {
            var eventGraph=_builder.TimerCancellationFailedGraph(_timerIdentity, "cause");
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.LastEvent;

            Assert.That(latestEvent, Is.EqualTo(new TimerCancellationFailedEvent(eventGraph.First())));
        }

        [Test]
        public void All_events_can_return_timer_fired_events()
        {
            var eventGraph = _builder.TimerFiredGraph(_timerIdentity, TimeSpan.FromSeconds(2));
            var timerItem = CreateTimerItemFor(eventGraph);

            var latestEvent = timerItem.AllEvents;

            Assert.That(latestEvent, Is.EqualTo(new[]{new TimerFiredEvent(eventGraph.First(),eventGraph)}));
        }

        [Test]
        public void Invalid_arguments_test()
        {
            var timerItem = (IFluentTimerItem) TimerItem.New(_timerIdentity, null);

            Assert.Throws<ArgumentNullException>(() => timerItem.OnCancelled(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnFailedCancellation(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnFired(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.OnStartFailure(null));
            Assert.Throws<ArgumentNullException>(() => timerItem.When(null));

            Assert.Throws<ArgumentException>(() => timerItem.AfterActivity(null,"1.0"));
            Assert.Throws<ArgumentException>(() => timerItem.AfterActivity("1.0", null));
            Assert.Throws<ArgumentException>(() => timerItem.AfterTimer(null));
            Assert.Throws<ArgumentException>(() => timerItem.AfterLambda(null));
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