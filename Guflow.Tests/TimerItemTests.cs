using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerItemTests
    {
        private readonly Identity _timerIdentity = Identity.Timer("timerName");

        [Test]
        public void By_default_schedule_timer_to_fire_immediately()
        {
            var timerItem = new TimerItem(_timerIdentity,null);

            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision,Is.EqualTo(new ScheduleTimerDecision(_timerIdentity,new TimeSpan())));
        }

        [Test]
        public void Can_be_configured_to_schedule_timer_to_fire_after_timeout()
        {
            var timerItem = new TimerItem(_timerIdentity, null);
            timerItem.FireAfter(TimeSpan.FromSeconds(3));
            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision, Is.EqualTo(new ScheduleTimerDecision(_timerIdentity, TimeSpan.FromSeconds(3))));
        }

        [Test]
        public void Return_empty_when_when_condiation_is_evaluated_to_false()
        {
            var timerItem = new TimerItem(_timerIdentity, null);
            timerItem.When(t => false);

            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision,Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void Latest_event_can_be_timer_started_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerStartedEventGraph(_timerIdentity,TimeSpan.FromSeconds(2)));
            var timerItem = new TimerItem(_timerIdentity, new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = timerItem.LatestEvent as TimerStartedEvent;

            Assert.NotNull(latestEvent, "Activity Item should have returned latest event");
        }

        [Test]
        public void Latest_event_can_be_timer_fired_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerFiredEventGraph(_timerIdentity, TimeSpan.FromSeconds(2)));
            var timerItem = new TimerItem(_timerIdentity, new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = timerItem.LatestEvent as TimerFiredEvent;

            Assert.NotNull(latestEvent, "Activity Item should have returned latest event");
        }

        [Test]
        public void Latest_event_can_be_timer_start_failed_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerStartFailedEventGraph(_timerIdentity, "cause"));
            var timerItem = new TimerItem(_timerIdentity, new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = timerItem.LatestEvent as TimerStartFailedEvent;

            Assert.NotNull(latestEvent, "Activity Item should have returned latest event");
        }

        [Test]
        public void Latest_event_can_be_timer_cancelled_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerCancelledEventGraph(_timerIdentity, TimeSpan.FromSeconds(2)));
            var timerItem = new TimerItem(_timerIdentity, new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = timerItem.LatestEvent as TimerCancelledEvent;

            Assert.NotNull(latestEvent, "Activity Item should have returned latest event");
        }

        [Test]
        public void Latest_event_can_be_timer_cancellation_failed_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateTimerCancellationFailedEventGraph(_timerIdentity, "cause"));
            var timerItem = new TimerItem(_timerIdentity, new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = timerItem.LatestEvent as TimerCancellationFailedEvent;

            Assert.NotNull(latestEvent, "Activity Item should have returned latest event");
        }

        private class TestWorkflowItems : IWorkflowItems
        {
            public TestWorkflowItems(IWorkflowHistoryEvents workflowHistoryEvents)
            {
                CurrentHistoryEvents = workflowHistoryEvents;
            }
            public IEnumerable<WorkflowItem> GetStartupWorkflowItems()
            {
                throw new System.NotImplementedException();
            }

            public IEnumerable<WorkflowItem> GetChildernOf(WorkflowItem item)
            {
                throw new System.NotImplementedException();
            }

            public WorkflowItem Find(Identity identity)
            {
                throw new System.NotImplementedException();
            }

            public IWorkflowHistoryEvents CurrentHistoryEvents { get; private set; }
        }
        
    }
}