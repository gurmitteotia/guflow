using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowHistoryEventsTests
    {
        private Mock<IWorkflowActions> _workflow;
        private WorkflowAction _interpretedWorkflowAction;
        private WorkflowDecision _expectedWorkflowDecision;
        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflowActions>();
            _expectedWorkflowDecision = new Mock<WorkflowDecision>(false,false).Object;
            _interpretedWorkflowAction = new TestWorkflowAction(_expectedWorkflowDecision);
        }
        
        [Test]
        public void Can_interpret_the_activity_completed_event()
        {
            _workflow.Setup(w => w.OnActivityCompletion(It.IsAny<ActivityCompletedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCompletedEventGraph();

            var workflowDecisions =historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] {_expectedWorkflowDecision}));
        }

        [Test]
        public void Can_interpret_the_activity_failed_event()
        {
            _workflow.Setup(w => w.OnActivityFailure(It.IsAny<ActivityFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_the_activity_timedout_event()
        {
            _workflow.Setup(w => w.OnActivityTimeout(It.IsAny<ActivityTimedoutEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityTimedoutEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_the_activity_cancelled_event()
        {
            _workflow.Setup(w => w.OnActivityCancelled(It.IsAny<ActivityCancelledEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCancelledEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_the_activity_cancellation_failed_event()
        {
            _workflow.Setup(w => w.OnActivityCancellationFailed(It.IsAny<ActivityCancellationFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCancellationFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_workflow_started_event()
        {
            _workflow.Setup(w => w.OnWorkflowStarted(It.IsAny<WorkflowStartedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateWorkflowStartedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_timer_fired_event()
        {
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerFiredEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_timer_failed_event()
        {
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_timer_cancelled_event()
        {
            _workflow.Setup(w => w.OnTimerCancelled(It.IsAny<TimerCancelledEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerCancelledEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_timer_cancellation_failed_event()
        {
            _workflow.Setup(w => w.OnTimerCancellationFailed(It.IsAny<TimerCancellationFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerCancellationFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }
        [Test]
        public void Can_interpret_workflow_signaled_event()
        {
            _workflow.Setup(w => w.OnWorkflowSignaled(It.IsAny<WorkflowSignaledEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateWorkflowSignaledEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_workflow_signaled_failed_event()
        {
            _workflow.Setup(w => w.OnWorkflowSignalFailed(It.IsAny<WorkflowSignalFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = new WorkflowHistoryEvents(new []{HistoryEventFactory.CreateWorkflowSignalFailedEvent("cause","wid","rid")});

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }
        [Test]
        public void Can_interpret_workflow_cancellation_requested_event()
        {
            _workflow.Setup(w => w.OnWorkflowCancellationRequested(It.IsAny<WorkflowCancellationRequestedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateWorkflowCancellationRequestedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }
        [Test]
        public void Can_interpret_workflow_completion_failed_event()
        {
            _workflow.Setup(w => w.OnWorkflowCompletionFailed(It.IsAny<WorkflowCompletionFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = new WorkflowHistoryEvents(new[]{HistoryEventFactory.CreateWorkflowCompletionFailureEvent("cause")});

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }
        [Test]
        public void Can_interpret_workflow_failure_failed_event()
        {
            _workflow.Setup(w => w.OnWorkflowFailureFailed(It.IsAny<WorkflowFailureFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = new WorkflowHistoryEvents(new[] { HistoryEventFactory.CreateWorkflowFailureFailedEvent("cause") });

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_workflow_cancel_request_failed_event()
        {
            _workflow.Setup(w => w.OnWorkflowCancelRequestFailed(It.IsAny<WorkflowCancelRequestFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = new WorkflowHistoryEvents(new[] { HistoryEventFactory.CreateWorkflowCancelRequestFailedEvent("cause") });

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Can_interpret_workflow_cancellation_failed_failed_event()
        {
            _workflow.Setup(w => w.OnWorkflowCancellationFailed(It.IsAny<WorkflowCancellationFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = new WorkflowHistoryEvents(new[] { HistoryEventFactory.CreateWorkflowCancellationFailedEvent("cause") });

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Should_return_empty_workflow_action_when_history_event_can_not_be_interpreted()
        {
            var historyEvents = CreateNotInterpretingEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Should_accumulate_the_interpreted_actions()
        {
            var timerFailedDecision = new Mock<WorkflowDecision>(false,false);
            var timerFailedAction = new TestWorkflowAction(timerFailedDecision.Object);
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns(_interpretedWorkflowAction);
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(timerFailedAction);
            var timerFiredAndFailedEvents = CreateTimerFireAndFailedEventGraph();

            var workflowDecisions = timerFiredAndFailedEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EquivalentTo(new[] { _expectedWorkflowDecision, timerFailedDecision.Object }));
        }

        [Test]
        public void Should_filter_out_duplicate_workflow_decisions()
        {
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns((WorkflowAction)null);
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(_interpretedWorkflowAction);
            var timerFiredAndFailedEvents = CreateTimerFireAndFailedEventGraph();

            var workflowDecisions = timerFiredAndFailedEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EquivalentTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Should_filter_out_null_workflow_decisions()
        {
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns(_interpretedWorkflowAction);
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(_interpretedWorkflowAction);
            var timerFiredAndFailedEvents = CreateTimerFireAndFailedEventGraph();

            var workflowDecisions = timerFiredAndFailedEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EquivalentTo(new[] { _expectedWorkflowDecision }));
        }

        [Test]
        public void Should_be_active_when_activity_is_just_started()
        {
            var activityStartedEventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New("activity", "1.0"), "id");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityStartedEventGraph);

            Assert.IsTrue(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_be_active_when_activity_is_just_scheduled()
        {
            var activityScheduledEventGraph = HistoryEventFactory.CreateActivityScheduledEventGraph(Identity.New("activity", "1.0"));
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityScheduledEventGraph);

            Assert.IsTrue(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_be_active_when_activity_cancellation_is_in_progress()
        {
            var activityCancelRequestedGraph = HistoryEventFactory.CreateActivityCancelRequestedGraph(Identity.New("activity", "1.0"),"id");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityCancelRequestedGraph);

            Assert.IsTrue(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_not_be_active_when_activity_is_completed()
        {
            var activityCompletedEventGraph =HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("activity", "1.0"), "id", "res");
            var workflowHistoryEvents = new WorkflowHistoryEvents(activityCompletedEventGraph);

            Assert.IsFalse(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_be_active_when_activity_is_just_started_after_failure()
        {
            var eventGraph = HistoryEventFactory.CreateActivityStartedEventGraph(Identity.New("activity", "1.0"), "id")
                                            .Concat(HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New("activity", "1.0"), "id", "res","detail"));
            var workflowHistoryEvents = new WorkflowHistoryEvents(eventGraph);

            Assert.IsTrue(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_be_active_when_timer_is_started()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerStartedEventGraph(Identity.Timer("id"),TimeSpan.FromSeconds(2));
            var workflowHistoryEvents = new WorkflowHistoryEvents(timerStartedEventGraph);

            Assert.IsTrue(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Should_not_be_active_when_timer_is_fired()
        {
            var timerStartedEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("id"), TimeSpan.FromSeconds(2));
            var workflowHistoryEvents = new WorkflowHistoryEvents(timerStartedEventGraph);

            Assert.IsFalse(workflowHistoryEvents.IsActive());
        }

        [Test]
        public void Can_return_all_marker_recorded_events()
        {
            var markerRecordedEventGraph = new[]
            {
                HistoryEventFactory.CreateMarkerRecordedEvent("name1", "detail1"),
                HistoryEventFactory.CreateMarkerRecordedEvent("name2", "detail2")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(markerRecordedEventGraph);
            var markerRecordedEvents = workflowHistoryEvents.AllMarkerRecordedEvents();

            Assert.That(markerRecordedEvents,Is.EqualTo(new []{new MarkerRecordedEvent(markerRecordedEventGraph.First()),
                new MarkerRecordedEvent(markerRecordedEventGraph.Last())}));
        }

        [Test]
        public void Can_return_all_signal_events()
        {
            var signalEventsGraph = new[]
            {
                HistoryEventFactory.CreateWorkflowSignaledEvent("name1", "input1"),
                HistoryEventFactory.CreateWorkflowSignaledEvent("name1", "input1","runid","wid")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(signalEventsGraph);
            var allSignalEvents = workflowHistoryEvents.AllSignalEvents();

            Assert.That(allSignalEvents,Is.EqualTo(new []{ new WorkflowSignaledEvent(signalEventsGraph.First()),new WorkflowSignaledEvent(signalEventsGraph.Last())}));
        }

        [Test]
        public void Can_return_all_cancellation_request()
        {
            var cancellationEventGraph = new[]
            {
                HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause"),
                HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause2","runid","wid")
            };
            var workflowHistoryEvents = new WorkflowHistoryEvents(cancellationEventGraph);
            var allWorkflowCancellationRequestedEvents = workflowHistoryEvents.AllWorkflowCancellationRequestedEvents();

            Assert.That(allWorkflowCancellationRequestedEvents, Is.EqualTo(new[] { new WorkflowCancellationRequestedEvent(cancellationEventGraph.First()), new WorkflowCancellationRequestedEvent(cancellationEventGraph.Last()) }));
        }

        private WorkflowHistoryEvents CreateActivityCompletedEventGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("activity", "1.0"), "id", "result").ToArray();
            return new WorkflowHistoryEvents(activityCompletedEventGraph);
        }
        private WorkflowHistoryEvents CreateActivityFailedEventGraph()
        {
            var activityFailedEventGraph = HistoryEventFactory.CreateActivityFailedEventGraph(Identity.New("activity", "1.0"), "id", "reason","detail");
            return new WorkflowHistoryEvents(activityFailedEventGraph);
        }
        private WorkflowHistoryEvents CreateActivityTimedoutEventGraph()
        {
            var activityTimedoutEventGraph = HistoryEventFactory.CreateActivityTimedoutEventGraph(Identity.New("activity", "1.0"), "id", "reason", "detail");
            return new WorkflowHistoryEvents(activityTimedoutEventGraph);
        }
        private WorkflowHistoryEvents CreateActivityCancelledEventGraph()
        {
            var activityCancelledEventGraph = HistoryEventFactory.CreateActivityCancelledEventGraph(Identity.New("activity", "1.0"), "id", "detail");
            return new WorkflowHistoryEvents(activityCancelledEventGraph);
        }
        private WorkflowHistoryEvents CreateActivityCancellationFailedEventGraph()
        {
            var activityCancellationFailedEventGraph = HistoryEventFactory.CreateActivityCancellationFailedEventGraph(Identity.New("activity", "1.0"), "detail");
            return new WorkflowHistoryEvents(activityCancellationFailedEventGraph);
        }
        private WorkflowHistoryEvents CreateWorkflowStartedEventGraph()
        {
            var workflowStartedEventGraph = HistoryEventFactory.CreateWorkflowStartedEventGraph();
            return new WorkflowHistoryEvents(workflowStartedEventGraph);
        }
        private WorkflowHistoryEvents CreateTimerFiredEventGraph()
        {
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer"), TimeSpan.FromSeconds(4));
            
            return new WorkflowHistoryEvents(timerFiredEventGraph);
        }
        private WorkflowHistoryEvents CreateTimerFailedEventGraph()
        {
            var timerFailedEventGraph = HistoryEventFactory.CreateTimerStartFailedEventGraph(Identity.Timer("timer"), "cause");
            return new WorkflowHistoryEvents(timerFailedEventGraph);
        }
        private WorkflowHistoryEvents CreateTimerCancelledEventGraph()
        {
            var timerCancelledEventGraph = HistoryEventFactory.CreateTimerCancelledEventGraph(Identity.Timer("timer"),TimeSpan.FromSeconds(4));
            return new WorkflowHistoryEvents(timerCancelledEventGraph);
        }
        private WorkflowHistoryEvents CreateTimerCancellationFailedEventGraph()
        {
            var timerCancellationFailedEventGraph = HistoryEventFactory.CreateTimerCancellationFailedEventGraph(Identity.Timer("timer"), "cause");
            return new WorkflowHistoryEvents(timerCancellationFailedEventGraph);
        }
        private WorkflowHistoryEvents CreateWorkflowSignaledEventGraph()
        {
            var workflowSignaledEvent = HistoryEventFactory.CreateWorkflowSignaledEvent("name","input");
            return new WorkflowHistoryEvents(new[]{workflowSignaledEvent});
        }
        private WorkflowHistoryEvents CreateWorkflowCancellationRequestedEventGraph()
        {
            var workflowCancellationRequestedEvent = HistoryEventFactory.CreateWorkflowCancellationRequestedEvent("cause");
            return new WorkflowHistoryEvents(new[] { workflowCancellationRequestedEvent });
        }
        private WorkflowHistoryEvents CreateNotInterpretingEventGraph()
        {
            var nonInterpretEvent = new HistoryEvent() {EventType = EventType.DecisionTaskCompleted};
            return new WorkflowHistoryEvents(new []{nonInterpretEvent});
        }
      
        private WorkflowHistoryEvents CreateTimerFireAndFailedEventGraph()
        {
            var timerStartedEventFileGraph =
                HistoryEventFactory.CreateTimerStartFailedEventGraph(Identity.Timer("timer"), "cause");
            var timerFiredEventGraph = HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer"),
                TimeSpan.FromSeconds(4));

            var combinedEventGraph = timerFiredEventGraph.Concat(timerStartedEventFileGraph);
            return new WorkflowHistoryEvents(combinedEventGraph);
        }

        private class TestWorkflowAction : WorkflowAction
        {
            private readonly WorkflowDecision _workflowDecision;

            public TestWorkflowAction(WorkflowDecision workflowDecision)
            {
                _workflowDecision = workflowDecision;
            }

            internal override IEnumerable<WorkflowDecision> GetDecisions()
            {
                return new[] {_workflowDecision};
            }
        }
    }
}