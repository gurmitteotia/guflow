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
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems(workflowInput));

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
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_custom_task_list()
        {
            const string taskList = "taskList";
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());
            activityItem.OnTaskList(a => taskList);

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskList, Is.EqualTo(taskList));
        }

        [Test]
        public void Does_not_schedule_activity_when_when_func_is_evaluated_to_false()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());
            activityItem.When(a => false);

            var decision = activityItem.GetScheduleDecision();

            Assert.That(decision, Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void By_default_schedule_activity_without_priority()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.HasValue, Is.False);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_priority()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());
            activityItem.WithPriority(a => 10);
            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.TaskPriority.Value, Is.EqualTo(10));
        }

        [Test]
        public void By_default_schedule_activity_with_empty_timeouts()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());

            var decision = (ScheduleActivityDecision)activityItem.GetScheduleDecision();

            Assert.That(decision.Timeouts.HeartbeatTimeout, Is.Null);
            Assert.That(decision.Timeouts.ScheduleToCloseTimeout,Is.Null);
            Assert.That(decision.Timeouts.ScheduleToStartTimeout,Is.Null);
            Assert.That(decision.Timeouts.StartToCloseTimeout,Is.Null);
        }

        [Test]
        public void Can_be_configured_to_schedule_activity_with_timeouts()
        {
            var activityItem = new ActivityItem("somename", "1.0", "name", new TestWorkflowItems());
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
        public void Parent_activity_test()
        {
            var workflowWithParentActivity = new TestWorkflow("parent1","1.0","pos");
            var childActivity = new ActivityItem("child","1.0",string.Empty,workflowWithParentActivity);
            childActivity.DependsOn("parent1", "1.0","pos");

            var parentActivities = childActivity.ParentActivities;
            
            Assert.That(parentActivities,Is.EquivalentTo(new []{new ActivityItem("parent1","1.0","pos",null)}));
            Assert.That(parentActivities.First().Name, Is.EqualTo("parent1"));
            Assert.That(parentActivities.First().Version, Is.EqualTo("1.0"));
            Assert.That(parentActivities.First().PositionalName, Is.EqualTo("pos"));
        }

        private class TestWorkflowItems : IWorkflowItems
        {
            public TestWorkflowItems(string workflowInput="")
            {
                CurrentHistoryEvents = new WorkflowHistoryEvents(HistoryEventFactory.CreateWorkflowStartedEventGraph(workflowInput));
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

        private class TestWorkflow : Workflow
        {
            public TestWorkflow(string parentActivityName,string parentActivityVersion, string postionalName)
            {
                AddActivity(parentActivityName, parentActivityVersion, postionalName);
            }
        }
    }
}