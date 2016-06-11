using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class ActivityItemTests
    {
        [Test]
        public void By_default_workflow_input_is_passed_as_activity_input()
        {
            const string workflowInput = "actvity";
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(new WorkflowHistoryEvents(HistoryEventFactory.CreateWorkflowStartedEventGraph(workflowInput))));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Input, Is.EqualTo(workflowInput));
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_input()
        {
            const string activityInput = "actvity";
            var activityItem = new ActivityItem("somename", "1.0", "name", null);
            activityItem.WithInput(a => activityInput);

            var decision = (ScheduleActivityDecision) activityItem.GetScheduleDecision();

            Assert.That(decision.Input,Is.EqualTo(activityInput));
        }

        [Test]
        public void By_default_schedule_activity_with_empty_task_list()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_task_list()
        {
            const string taskList = "taskList";
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));
            activityItem.OnTaskList(a => taskList);

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.EqualTo(taskList));
        }

        [Test]
        public void Does_not_schedule_activity_when_when_func_is_evaluated_to_false()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));
            activityItem.When(a => false);

            var decision = activityItem.GetScheduleDecision();

            Assert.That(decision, Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void By_default_schedule_activity_without_priority()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.HasValue, Is.False);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_priority()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));
            activityItem.WithPriority(a => 10);
            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.Value, Is.EqualTo(10));
        }

        [Test]
        public void By_default_schedule_activity_with_empty_timeouts()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.Null);
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout,Is.Null);
            Assert.That(decision.Timeouts.ScheduleToStartTimeout,Is.Null);
            Assert.That(decision.Timeouts.StartToCloseTimeout,Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_timeouts()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(null));
            activityItem.WithTimeouts(
                a =>
                    new ScheduleActivityTimeouts()
                    {
                        HeartbeatTimeout = TimeSpan.FromSeconds(2),
                        ScheduleToCloseTimeout = TimeSpan.FromSeconds(3),
                        ScheduleToStartTimeout = TimeSpan.FromSeconds(4),
                        StartToCloseTimeout = TimeSpan.FromSeconds(5)
                    });
            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(3)));
            Assert.That(decision.Timeouts.ScheduleToStartTimeout, Is.EqualTo(TimeSpan.FromSeconds(4)));
            Assert.That(decision.Timeouts.StartToCloseTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void Parent_activities_test()
        {
            var workflowWithParentActivity = new WorkflowWithParentActivity("parent1","1.0","pos");
            var childActivity = new ActivityItem("child","1.0",string.Empty,workflowWithParentActivity);
            childActivity.DependsOn("parent1", "1.0","pos");

            var parentActivities = childActivity.ParentActivities;
            
            Assert.That(parentActivities,Is.EquivalentTo(new []{new ActivityItem("parent1","1.0","pos",null)}));
            Assert.That(parentActivities.First().Name, Is.EqualTo("parent1"));
            Assert.That(parentActivities.First().Version, Is.EqualTo("1.0"));
            Assert.That(parentActivities.First().PositionalName, Is.EqualTo("pos"));
        }

        [Test]
        public void Parent_timers_test()
        {
            var workflowWithParentActivity = new WorkflowWithParentTimer("parent1");
            var childActivity = new ActivityItem("child", "1.0", string.Empty, workflowWithParentActivity);
            childActivity.DependsOn("parent1");

            var parentActivities = childActivity.ParentTimers;

            Assert.That(parentActivities, Is.EquivalentTo(new[] { new TimerItem("parent1", null),  }));
            Assert.That(parentActivities.First().Name, Is.EqualTo("parent1"));
        }

        [Test]
        public void Latest_event_can_be_activity_started_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New("somename","1.0","name"),"id"));
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = activityItem.LatestEvent as ActivityStartedEvent;

            Assert.NotNull(latestEvent,"Activity Item should have returned latest event");
        }

        [Test]
        public void Latest_event_can_be_activity_scheduled_event()
        {
            var workflowHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateActivityScheduledEventGraph(Identity.New("somename", "1.0", "name")));
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(workflowHistoryEvents));

            var latestEvent = activityItem.LatestEvent as ActivityScheduledEvent;

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

        private class WorkflowWithParentActivity : Workflow
        {
            public WorkflowWithParentActivity(string parentActivityName,string parentActivityVersion, string postionalName)
            {
                ScheduleActivity(parentActivityName, parentActivityVersion, postionalName);
            }
        }
        private class WorkflowWithParentTimer : Workflow
        {
            public WorkflowWithParentTimer(string parentTimerName)
            {
                ScheudleTimer(parentTimerName);
            }
        }
    }
}