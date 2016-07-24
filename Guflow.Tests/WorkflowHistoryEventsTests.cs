using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowHistoryEventsTests
    {
        private Mock<IWorkflowActions> _workflow;
        private WorkflowAction _interpretedWorkflowAction;
        private Mock<WorkflowDecision> _expectedWorkflowDecision;
        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflowActions>();
            _expectedWorkflowDecision = new Mock<WorkflowDecision>(false,false);
            _interpretedWorkflowAction = new TestWorkflowAction(_expectedWorkflowDecision.Object);
        }
        
        [Test]
        public void Can_interpret_the_activity_completed_event()
        {
            _workflow.Setup(w => w.OnActivityCompletion(It.IsAny<ActivityCompletedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCompletedEventGraph();

            var workflowDecisions =historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] {_expectedWorkflowDecision.Object}));
        }

        [Test]
        public void Can_interpret_the_activity_failed_event()
        {
            _workflow.Setup(w => w.OnActivityFailure(It.IsAny<ActivityFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_timedout_event()
        {
            _workflow.Setup(w => w.OnActivityTimeout(It.IsAny<ActivityTimedoutEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityTimedoutEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_cancelled_event()
        {
            _workflow.Setup(w => w.OnActivityCancelled(It.IsAny<ActivityCancelledEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCancelledEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_cancellation_failed_event()
        {
            _workflow.Setup(w => w.OnActivityCancellationFailed(It.IsAny<ActivityCancellationFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateActivityCancellationFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_workflow_started_event()
        {
            _workflow.Setup(w => w.OnWorkflowStarted(It.IsAny<WorkflowStartedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateWorkflowStartedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_timer_fired_event()
        {
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerFiredEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_timer_failed_event()
        {
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_timer_cancelled_event()
        {
            _workflow.Setup(w => w.OnTimerCancelled(It.IsAny<TimerCancelledEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerCancelledEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
        }

        [Test]
        public void Can_interpret_timer_cancellation_failed_event()
        {
            _workflow.Setup(w => w.OnTimerCancellationFailed(It.IsAny<TimerCancellationFailedEvent>())).Returns(_interpretedWorkflowAction);
            var historyEvents = CreateTimerCancellationFailedEventGraph();

            var workflowDecisions = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { _expectedWorkflowDecision.Object }));
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

            Assert.That(workflowDecisions, Is.EquivalentTo(new[] { _expectedWorkflowDecision.Object, timerFailedDecision.Object }));
        }

        [Test]
        public void Should_filter_out_duplicate_workflow_decisions()
        {
            _workflow.Setup(w => w.OnTimerFired(It.IsAny<TimerFiredEvent>())).Returns(_interpretedWorkflowAction);
            _workflow.Setup(w => w.OnTimerStartFailure(It.IsAny<TimerStartFailedEvent>())).Returns(_interpretedWorkflowAction);
            var timerFiredAndFailedEvents = CreateTimerFireAndFailedEventGraph();

            var workflowDecisions = timerFiredAndFailedEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowDecisions, Is.EquivalentTo(new[] { _expectedWorkflowDecision.Object }));
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

        private WorkflowHistoryEvents CreateNotInterpretingEventGraph()
        {
            var nonInterpretEvent = new HistoryEvent() {EventType = EventType.DecisionTaskCompleted};
            return new WorkflowHistoryEvents(new []{nonInterpretEvent});
        }
      
        private WorkflowHistoryEvents CreateTimerFireAndFailedEventGraph()
        {
            var combinedEventGraph = HistoryEventFactory.CreateTimerStartFailedEventGraph(Identity.Timer("timer"), "cause")
                .Concat(HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer"), TimeSpan.FromSeconds(4)));
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