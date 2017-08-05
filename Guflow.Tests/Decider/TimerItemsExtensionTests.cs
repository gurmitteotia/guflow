using System;
using System.Collections.Generic;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerItemsExtensionTests
    {
        [Test]
        public void First_test()
        {
            var timerItems = new[] { CreateTimer("name1"), CreateTimer("name2") };

            Assert.That(timerItems.First("name1"), Is.EqualTo(timerItems[0]));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            IEnumerable<ITimerItem> timerItems = null;

            Assert.Throws<ArgumentNullException>(() => timerItems.First(""));
        }
        private static ITimerItem CreateTimer(string name)
        {
            return TimerItem.New(Identity.Timer(name), Mock.Of<IWorkflow>());
        }
    }
}