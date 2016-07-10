using System;
using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTests
    {
        private Mock<IWorkflowHistoryEvents> _workflowHistoryEvents;

        [SetUp]
        public void Setup()
        {
            _workflowHistoryEvents = new Mock<IWorkflowHistoryEvents>();
        }
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

        [Test]
        public void Should_return_complete_workflow_decision_when_only_propose_to_complete_workflow_decision_is_generated()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] {new CompleteWorkflowDecision("complete", true)});
            
            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions,Is.EqualTo(new []{new CompleteWorkflowDecision("complete",true)}));
        }

        [Test]
        public void Should_filter_out_propose_to_complete_workflow_decision_when_it_is_generated_along_with_other_decisions()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[] { new CompleteWorkflowDecision("complete", true),
                                                                                new ScheduleActivityDecision(Identity.New("something","1.0"))});

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New("something","1.0"))}));
        }

        [Test]
        public void Should_filter_out_scheduling_decisions_when_generated_along_complet_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[] { new CompleteWorkflowDecision("complete"),
                                                                                new ScheduleActivityDecision(Identity.New("something","1.0"))});

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
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
                ScheduleActivity("_timerName", "version").After("ParentName", "parentVer");
            }
        }
        private class WorkflowWithNonExistentParentTimerItem : Workflow
        {
            public WorkflowWithNonExistentParentTimerItem()
            {
                ScheduleActivity("_timerName", "version").After("ParentName");
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

        private class StubWorkflow : Workflow
        {
            
        }
    }

}