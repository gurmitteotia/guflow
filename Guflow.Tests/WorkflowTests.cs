using System;
using System.Collections.Generic;
using System.Linq;
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
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(It.IsAny<Workflow>())).Returns(new WorkflowDecision[] { });
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

            Assert.That(activity, Is.EqualTo(new TimerItem(Identity.Timer("Timer1"), workflow)));
        }

        [Test]
        public void Should_return_complete_workflow_decision_when_only_propose_to_complete_workflow_decision_is_generated()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete", true) });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Should_filter_out_propose_to_complete_workflow_decision_when_it_is_generated_along_with_other_decisions()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[] { new CompleteWorkflowDecision("complete", true),
                                                                                new ScheduleActivityDecision(Identity.New("something","1.0"))});

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New("something", "1.0")) }));
        }

        [Test]
        public void Should_filter_out_propose_to_complete_workflow_decision_and_any_scheduling_decision_when_it_is_generated_along_with_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete", true),
                                                                                new CompleteWorkflowDecision("complete3")}));

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete3") }));
        }

        [Test]
        public void Should_filter_out_scheduling_decisions_when_generated_along_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete") }));

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Should_filter_out_activity_scheduling_decisions_when_generated_along_with_fail_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new FailWorkflowDecision("reason", "detail") }));

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Should_filter_out_activity_scheduling_decisions_when_generated_along_with_cancel_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(AllNonCompletingDecisions().Concat(new[] { new CancelWorkflowDecision("detail") }));

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Should_return_fail_workflow_decision_when_multiple_close_workflow_decisions_are_generated()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
                new CompleteWorkflowDecision("result2",true),
                new FailWorkflowDecision("reason","detail"), 
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Should_return_cancel_workflow_decision_when_it_generated_along_with_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Should_return_complete_workflow_decision_when_it_generated_along_with_a_proposed_complete_workflow_decision()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_completion()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false,false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_failure()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new FailWorkflowDecision("reason","detail"), 
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_cancellation()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CancelWorkflowDecision("detail"), 
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }
        [Test]
        public void Ignores_when_null_workflow_action_is_returned_during_completion()
        {
            var workflow = new WorkflowToReturnNullActionOnClosing();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result2",true),
            });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }
        [Test]
        public void Should_return_empty_decisions_when_only_propose_to_complete_workflow_decision_is_generated_and_workflow_is_active()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete", true) });
            _workflowHistoryEvents.Setup(h => h.IsActive()).Returns(true);

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Should_filter_out_empty_workflow_decisions()
        {
            var workflow = new StubWorkflow();
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("complete"), WorkflowDecision.Empty });
            _workflowHistoryEvents.Setup(h => h.IsActive()).Returns(true);

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[]{new CompleteWorkflowDecision("complete")}));
        }

        [Test]
        public void Workflow_execution_can_return_marker_decisions()
        {
            var workflow = new WorkflowWithMarker("name","detail");

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions,Is.EqualTo(new []{new RecordMarkerDecision("name","detail")}));
        }

        [Test]
        public void Return_markers_decision_when_generated_along_with_propose_to_close_workflow()
        {
            var workflow = new WorkflowWithMarker("name", "detail");
            _workflowHistoryEvents.Setup(w => w.InterpretNewEventsFor(workflow)).Returns(new[] { new CompleteWorkflowDecision("detail",true), });

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new RecordMarkerDecision("name", "detail") }));
        }

        [Test]
        public void Markers_are_cleared_after_execution()
        {
            var workflow = new WorkflowWithMarker("name", "detail");
            workflow.ExecuteFor(_workflowHistoryEvents.Object);
 
            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Workflow_execution_can_return_signal_decision()
        {
            var workflow = new WorkflowWithSignal("signalName","signalDetail","workflowId","runid");

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new SignalWorkflowDecision("signalName", "signalDetail", "workflowId", "runid")}));
        }
        [Test]
        public void Signals_are_cleared_after_execution()
        {
            var workflow = new WorkflowWithSignal("signalName", "signalDetail", "workflowId", "runid");
            workflow.ExecuteFor(_workflowHistoryEvents.Object);

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Workflow_can_reply_to_a_signal_during_execution()
        {
            var workflow = new WorkflowToReplyToSignal("signalName", "signalDetail");
            IWorkflowActions actions = workflow;
            actions.OnWorkflowSignaled(new WorkflowSignaledEvent(HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input", "runid","wid")));

            var workflowDecisions = workflow.ExecuteFor(_workflowHistoryEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new []{new SignalWorkflowDecision("signalName","signalDetail","wid","runid")}));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new WithNullActivityName());
            Assert.Throws<ArgumentException>(() => new WithNullActivityVersion());
            Assert.Throws<ArgumentException>(() => new WithNullTimerName());
            Assert.Throws<ArgumentException>(() => new WorkflowWithMarker(null, "detail"));
            Assert.Throws<ArgumentException>(() => new WorkflowWithSignal(null, "detail", "id", "id1"));
            Assert.Throws<ArgumentException>(() => new WorkflowWithSignal("signalName", "detail", null, "id1"));
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

        private class WorkflowWithMarker : Workflow
        {
            public WorkflowWithMarker(string markerName,string markerDetail)
            {
                RecordMarker(markerName, markerDetail);
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
        private class WorkflowWithSignal : Workflow
        {
            public WorkflowWithSignal(string signalName, string signalDetail, string workflowid, string runid)
            {
                Signal(signalName, signalDetail).SendTo(workflowid, runid);
            }
        }

        private class WorkflowToReplyToSignal : Workflow
        {
            private readonly string _signalName;
            private readonly string _signalInput;
            public WorkflowToReplyToSignal(string signalName, string signalInput)
            {
                _signalName = signalName;
                _signalInput = signalInput;
            }

            protected override WorkflowAction OnSignal(WorkflowSignaledEvent workflowSignaledEvent)
            {
                Signal(_signalName, _signalInput).ReplyTo(workflowSignaledEvent);
                return Ignore();
            }
        }
    }

}