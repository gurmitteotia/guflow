using System;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerItemTests
    {
        [Test]
        public void By_default_schedule_timer_to_fire_immediately()
        {
            var timerItem = new TimerItem(Identity.Timer("timerName"),null);

            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision,Is.EqualTo(new ScheduleTimerDecision(Identity.Timer("timerName"),new TimeSpan())));
        }

        [Test]
        public void Can_be_configured_to_schedule_timer_to_fire_after_timeout()
        {
            var timerItem = new TimerItem(Identity.Timer("timerName"), null);
            timerItem.FireAfter(TimeSpan.FromSeconds(3));
            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision, Is.EqualTo(new ScheduleTimerDecision(Identity.Timer("timerName"), TimeSpan.FromSeconds(3))));
        }

        [Test]
        public void Return_empty_when_when_condiation_is_evaluated_to_false()
        {
            var timerItem = new TimerItem(Identity.Timer("timerName"), null);
            timerItem.When(t => false);

            var decision = timerItem.GetScheduleDecision();

            Assert.That(decision,Is.EqualTo(WorkflowDecision.Empty));
        }
        
    }
}