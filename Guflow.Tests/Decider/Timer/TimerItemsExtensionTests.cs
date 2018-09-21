// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class TimerItemsExtensionTests
    {
        private const string Timer1 = "name1";
        private EventGraphBuilder _eventGraphBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
        }

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

        [Test]
        public void IsFired_when_last_event_is_fired()
        {
            var completedGraph = _eventGraphBuilder.TimerFiredGraph(Identity.Timer(Timer1), TimeSpan.Zero);
            var timer = CreateTimerItemFor(completedGraph);

            Assert.IsTrue(timer.IsFired());
        }
        [Test]
        public void IsFired_when_last_event_is_started()
        {
            var completedGraph = _eventGraphBuilder.TimerStartedGraph(Identity.Timer(Timer1), TimeSpan.Zero);
            var timer = CreateTimerItemFor(completedGraph);

            Assert.IsFalse(timer.IsFired());
        }

        [Test]
        public void IsCancelled_when_last_event_is_cancelled()
        {
            var completedGraph = _eventGraphBuilder.TimerCancelledGraph(Identity.Timer(Timer1), TimeSpan.Zero);
            var timer = CreateTimerItemFor(completedGraph);

            Assert.IsTrue(timer.IsCancelled());
        }
        [Test]
        public void IsCancelled_when_last_event_is_started()
        {
            var completedGraph = _eventGraphBuilder.TimerStartedGraph(Identity.Timer(Timer1), TimeSpan.Zero);
            var timer = CreateTimerItemFor(completedGraph);

            Assert.IsFalse(timer.IsCancelled());
        }

        private static ITimerItem CreateTimer(string name)
        {
            return TimerItem.New(Identity.Timer(name), Mock.Of<IWorkflow>());
        }

        private TimerItem CreateTimerItemFor(IEnumerable<HistoryEvent> eventGraph)
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);
            var workflow = new Mock<IWorkflow>();
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(workflowHistoryEvents);
            return TimerItem.New(Identity.Timer(Timer1), workflow.Object);
        }
    }
}