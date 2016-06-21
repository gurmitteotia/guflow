using System;
using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTests
    {
        [Test]
        public void Throws_exception_when_adding_the_same_item_again()
        {
            Assert.Throws<DuplicateItemException>(()=>new WorkflowWithSameActivity());
            Assert.Throws<DuplicateItemException>(() => new WorkflowWithSameTimer());
        }

        [Test]
        public void Throws_exception_when_parent_item_is_not_found()
        {
            Assert.Throws<ParentItemMissingException>(() => new WorkflowWithNonExistentParentActivityItem());
            Assert.Throws<ParentItemMissingException>(()=>new WorkflowWithNonExistentParentTimerItem());
        }

        [Test]
        public void Activityof_test()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("Activity1", "1.0"),"id","result");
            var activityCompletedEvent = new ActivityCompletedEvent(eventGraph.First(),eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetActivityOf(activityCompletedEvent);

            Assert.That(activity,Is.EqualTo(new ActivityItem(Identity.New("Activity1","1.0"),workflow)));
        }

        [Test]
        public void Timerof_test()
        {
            var eventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("Timer1"),TimeSpan.FromSeconds(2));
            var activityCompletedEvent = new TimerFiredEvent(eventGraph.First(), eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetTimerOf(activityCompletedEvent);

            Assert.That(activity, Is.EqualTo(new TimerItem(Identity.Timer("Timer1"), workflow)));
        }

        private class WorkflowWithSameActivity : Workflow
        {
            public WorkflowWithSameActivity()
            {
                ScheduleActivity("Download", "1.0");
                ScheduleActivity("Download", "1.0");
            }
        }
        private class WorkflowWithSameTimer : Workflow
        {
            public WorkflowWithSameTimer()
            {
                ScheduleTimer("Download");
                ScheduleTimer("Download");
            }
        }

        private class WorkflowWithNonExistentParentActivityItem : Workflow
        {
            public WorkflowWithNonExistentParentActivityItem()
            {
                ScheduleActivity("_timerName", "version").DependsOn("ParentName", "parentVer");
            }
        }
        private class WorkflowWithNonExistentParentTimerItem : Workflow
        {
            public WorkflowWithNonExistentParentTimerItem()
            {
                ScheduleActivity("_timerName", "version").DependsOn("ParentName");
            }
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleTimer("Timer1");
                ScheduleActivity("Activity1", "1.0");
            }

            public IActivityItem GetActivityOf(WorkflowItemEvent workflowItemEvent)
            {
                return ActivityOf(workflowItemEvent);
            }

            public ITimerItem GetTimerOf(WorkflowItemEvent workflowItemEvent)
            {
                return TimerOf(workflowItemEvent);
            }
        }
    }

}