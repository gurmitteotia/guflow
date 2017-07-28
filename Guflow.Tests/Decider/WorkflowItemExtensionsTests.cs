using System;
using System.Linq;
using Guflow.Decider;
using Guflow.Worker;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowItemExtensionsTests
    {
        private Mock<IWorkflowItem> _workflowItem;

        [SetUp]
        public void Setup()
        {
            _workflowItem = new Mock<IWorkflowItem>();
        }

        [Test]
        public void Null_argument_tests()
        {
            IWorkflowItem item = null;
            Assert.Throws<ArgumentNullException>(() => item.AllEventsOf<ActivityCompletedEvent>());
            Assert.Throws<ArgumentNullException>(() => item.AllEventsOf(typeof(ActivityCompletedEvent)) );
        }

        [Test]
        public void Can_filter_out_all_events()
        {
            var allEvents = new WorkflowItemEvent[] {CreateCompletedEvent(), CreateFailedEvent()};
            _workflowItem.SetupGet(w => w.AllEvents).Returns(allEvents);

            Assert.That(_workflowItem.Object.AllEventsOf<ActivityCompletedEvent>(), 
                Is.EqualTo(new []{allEvents[0]}));
            Assert.That(_workflowItem.Object.AllEventsOf(typeof(ActivityFailedEvent)),
                Is.EqualTo(new[] { allEvents[1] }));
        }

        [Test]
        public void Can_filter_out_a_parent_activity()
        {
            var parentActivities = new[]
            {
                new ActivityItem(Identity.New("name1", "1.0"), Mock.Of<IWorkflow>()),
                new ActivityItem(Identity.New("name2", "1.0", "pos"), Mock.Of<IWorkflow>())
            };
            _workflowItem.SetupGet(w => w.ParentActivities).Returns(parentActivities);

            Assert.That(_workflowItem.Object.ParentActivity("name1", "1.0"), Is.EqualTo(parentActivities[0]));
            Assert.That(_workflowItem.Object.ParentActivity<Activity2>("pos"), Is.EqualTo(parentActivities[1]));
        }

        [Test]
        public void Can_get_single_parent_activity()
        {
            var parentActivities = new[]
            {
                new ActivityItem(Identity.New("name1", "1.0"), Mock.Of<IWorkflow>()),
            };
            _workflowItem.SetupGet(w => w.ParentActivities).Returns(parentActivities);

            Assert.That(_workflowItem.Object.ParentActivity(), Is.EqualTo(parentActivities[0]));
        }

        [Test]
        public void Can_filter_out_parent_timer()
        {
            var parentTimers = new[]
            {
                TimerItem.New(Identity.Timer("name1"), Mock.Of<IWorkflow>()),
                TimerItem.New(Identity.Timer("name2"), Mock.Of<IWorkflow>())
            };
            _workflowItem.SetupGet(w => w.ParentTimers).Returns(parentTimers);

            Assert.That(_workflowItem.Object.ParentTimer("name1"), Is.EqualTo(parentTimers[0]));
        }

        [Test]
        public void Can_get_single_parent_timer()
        {
            var parentTimers = new[]
            {
                TimerItem.New(Identity.Timer("name1"), Mock.Of<IWorkflow>()),
            };
            _workflowItem.SetupGet(w => w.ParentTimers).Returns(parentTimers);

            Assert.That(_workflowItem.Object.ParentTimer(), Is.EqualTo(parentTimers[0]));
        }

        private static ActivityCompletedEvent CreateCompletedEvent()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("name", "1.0"), "id",
                "result");
            return new ActivityCompletedEvent(eventGraph.First(), eventGraph);
        }

        private static ActivityFailedEvent CreateFailedEvent()
        {
            var eventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New("name", "1.0"), "id","reason", "detail");
            return new ActivityFailedEvent(eventGraph.First(), eventGraph);
        }

        [ActivityDescription("1.0", Name = "name2")]
        private class Activity2 : Activity
        {
            
        }
    }
}