﻿using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowTests
    {
        private Mock<IWorkflowEvents> _workflowEvents;

        [SetUp]
        public void Setup()
        {
            _workflowEvents = new Mock<IWorkflowEvents>();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(It.IsAny<Workflow>())).Returns(new WorkflowDecision[] { });
        }
        [Test]
        public void Throws_exception_when_adding_the_same_item_again()
        {
            Assert.Throws<DuplicateItemException>(() => new WorkflowWithSameActivity());
            Assert.Throws<DuplicateItemException>(() => new WorkflowWithSameTimer());
        }

        [Test]
        public void Throws_exception_when_parent_item_is_not_found()
        {
            Assert.Throws<ParentItemMissingException>(() => new WorkflowWithNonExistentParentActivityItem());
            Assert.Throws<ParentItemMissingException>(() => new WorkflowWithNonExistentParentTimerItem());
        }

        [Test]
        public void Throws_exception_when_scheduling_activity_depends_on_itself()
        {
            Assert.Throws<CyclicDependencyException>(() => new WorkflowWithActivityToParentItself());
        }

        [Test]
        public void Activityof_test()
        {
            var eventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("Activity1", "1.0"), "id", "result");
            var activityCompletedEvent = new ActivityCompletedEvent(eventGraph.First(), eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetActivityOf(activityCompletedEvent);

            Assert.That(activity, Is.EqualTo(new ActivityItem(Identity.New("Activity1", "1.0"), workflow)));
        }

        [Test]
        public void Timerof_test()
        {
            var eventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("Timer1"), TimeSpan.FromSeconds(2));
            var activityCompletedEvent = new TimerFiredEvent(eventGraph.First(), eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetTimerOf(activityCompletedEvent);

            Assert.That(activity, Is.EqualTo(TimerItem.New(Identity.Timer("Timer1"), workflow)));
        }

        [Test]
        public void Returns_complete_workflow_decision_when_only_propose_to_complete_workflow_decision_is_generated()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete", true) });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Filters_out_propose_to_complete_workflow_decision_when_it_is_generated_along_with_other_decisions()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[] { new CompleteWorkflowDecision("complete", true),
                                                                                new ScheduleActivityDecision(Identity.New("something","1.0"))});

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New("something", "1.0")) }));
        }

        [Test]
        public void Filters_out_propose_to_complete_workflow_decision_and_any_scheduling_decision_when_it_is_generated_along_with_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete", true),
                                                                                new CompleteWorkflowDecision("complete3")}));

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete3") }));
        }

        [Test]
        public void Filters_out_scheduling_decisions_when_generated_along_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete") }));

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Filters_out_activity_scheduling_decisions_when_generated_along_with_fail_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new FailWorkflowDecision("reason", "detail") }));

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Filters_out_activity_scheduling_decisions_when_generated_along_with_cancel_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CancelWorkflowDecision("detail") }));

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Returns_fail_workflow_decision_when_multiple_close_workflow_decisions_are_generated()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
                new CompleteWorkflowDecision("result2",true),
                new FailWorkflowDecision("reason","detail"), 
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Returns_cancel_workflow_decision_when_it_generated_along_with_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Return_complete_workflow_decision_when_it_generated_along_with_a_proposed_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_completion()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false,false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_failure()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new FailWorkflowDecision("reason","detail"), 
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_cancellation()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CancelWorkflowDecision("detail"), 
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }
        [Test]
        public void Ignores_when_null_workflow_action_is_returned_during_completion()
        {
            var workflow = new WorkflowToReturnNullActionOnClosing();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.Empty);
        }
        [Test]
        public void Returns_empty_decisions_when_only_propose_to_complete_workflow_decision_is_generated_and_workflow_is_active()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete", true) });
            _workflowEvents.Setup(h => h.IsActive()).Returns(true);

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Filters_out_empty_workflow_decisions()
        {
            var workflow = new StubWorkflow();
            _workflowEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete"), WorkflowDecision.Empty });
            _workflowEvents.Setup(h => h.IsActive()).Returns(true);

            var workflowDecisions = workflow.NewExecutionFor(_workflowEvents.Object).Execute();

            Assert.That(workflowDecisions, Is.EqualTo(new[]{new CompleteWorkflowDecision("complete")}));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new WithNullActivityName());
            Assert.Throws<ArgumentException>(() => new WithNullActivityVersion());
            Assert.Throws<ArgumentException>(() => new WithNullTimerName());
        }

        private IEnumerable<WorkflowDecision> AllNonCompletingDecisions()
        {
            return new WorkflowDecision[]
            {
                new ScheduleActivityDecision(Identity.New("id", "1.0")),
                new ScheduleTimerDecision(Identity.Timer("timer"), TimeSpan.FromSeconds(2)),
                new CancelActivityDecision(Identity.New("newid", "1.0")),
                new CancelTimerDecision(Identity.Timer("first")),
            };
        }

        private class WorkflowWithSameActivity : Workflow
        {
            public WorkflowWithSameActivity()
            {
                ScheduleActivity("Download", "1.0");
                ScheduleActivity("Download", "1.0");
            }
        }
        private class WorkflowWithActivityToParentItself : Workflow
        {
            public WorkflowWithActivityToParentItself()
            {
                ScheduleActivity("Download", "1.0").After("Download", "1.0");
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
        private class WorkflowToReturnCustomActionOnClosing : Workflow
        {
            private readonly Mock<WorkflowAction> _customAction; 
            public WorkflowToReturnCustomActionOnClosing(WorkflowDecision workflowDecision)
            {
                _customAction = new Mock<WorkflowAction>();
                _customAction.Setup(w => w.GetDecisions()).Returns(new[] {workflowDecision});
            }
            protected override WorkflowAction DuringCompletion(string result)
            {
                return _customAction.Object;
            }

            protected override WorkflowAction DuringFailure(string reason, string detail)
            {
                return _customAction.Object;
            }

            protected override WorkflowAction DuringCancellation(string detail)
            {
                return _customAction.Object;
            }
        }

        private class WorkflowToReturnNullActionOnClosing : Workflow
        {
            protected override WorkflowAction DuringCompletion(string result)
            {
                return null;
            }
        }

        private class WithNullActivityName : Workflow
        {
            public WithNullActivityName()
            {
                ScheduleActivity(null, "1.0");
            }
        }
        private class WithNullActivityVersion : Workflow
        {
            public WithNullActivityVersion()
            {
                ScheduleActivity("act", null);
            }
        }
        private class WithNullTimerName : Workflow
        {
            public WithNullTimerName()
            {
                ScheduleTimer(null);
            }
        }
    }

}