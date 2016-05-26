using System;
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
        private Mock<IWorkflow> _workflow;
        private Mock<WorkflowAction> _expectedWorkflowAction;
        [SetUp]
        public void Setup()
        {
            _workflow = new Mock<IWorkflow>();
            _expectedWorkflowAction = new Mock<WorkflowAction>();
        }
        
        [Test]
        public void Can_interpret_the_activity_completed_event()
        {
            _workflow.Setup(w => w.ActivityCompleted(It.IsAny<ActivityCompletedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateActivityCompletedEventGraph();

            var workflowAction =historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] {_expectedWorkflowAction.Object}));
        }

        [Test]
        public void Can_interpret_the_activity_failed_event()
        {
            _workflow.Setup(w => w.ActivityFailed(It.IsAny<ActivityFailedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateActivityFailedEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_timedout_event()
        {
            _workflow.Setup(w => w.ActivityTimedout(It.IsAny<ActivityTimedoutEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateActivityTimedoutEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_cancelled_event()
        {
            _workflow.Setup(w => w.ActivityCancelled(It.IsAny<ActivityCancelledEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateActivityCancelledEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_the_activity_cancellation_failed_event()
        {
            _workflow.Setup(w => w.ActivityCancellationFailed(It.IsAny<ActivityCancellationFailedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateActivityCancellationFailedEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_workflow_started_event()
        {
            _workflow.Setup(w => w.WorkflowStarted(It.IsAny<WorkflowStartedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateWorkflowStartedEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_timer_fired_event()
        {
            _workflow.Setup(w => w.TimerFired(It.IsAny<TimerFiredEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateTimerFiredEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_timer_failed_event()
        {
            _workflow.Setup(w => w.TimerFailed(It.IsAny<TimerFailedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateTimerFailedEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_timer_cancelled_event()
        {
            _workflow.Setup(w => w.TimerCancelled(It.IsAny<TimerCancelledEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateTimerCancelledEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Can_interpret_timer_cancellation_failed_event()
        {
            _workflow.Setup(w => w.TimerCancellationFailed(It.IsAny<TimerCancellationFailedEvent>())).Returns(_expectedWorkflowAction.Object);
            var historyEvents = CreateTimerCancellationFailedEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.EqualTo(new[] { _expectedWorkflowAction.Object }));
        }

        [Test]
        public void Should_return_empty_workflow_action_when_history_event_can_not_be_interpreted()
        {
            var historyEvents = CreateNotInterpretingEventGraph();

            var workflowAction = historyEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowAction, Is.Empty);
        }

        [Test]
        public void Should_accumulate_the_interpreted_actions()
        {
            _workflow.Setup(w => w.TimerFired(It.IsAny<TimerFiredEvent>())).Returns(_expectedWorkflowAction.Object);
            _workflow.Setup(w => w.TimerFailed(It.IsAny<TimerFailedEvent>())).Returns(_expectedWorkflowAction.Object);
            var timerFiredAndFailedEvents = CreateTimerFireAndFailedEventGraph();

            var workflowActions = timerFiredAndFailedEvents.InterpretNewEventsFor(_workflow.Object);

            Assert.That(workflowActions, Is.EquivalentTo(new[] { _expectedWorkflowAction.Object, _expectedWorkflowAction.Object }));
        }
        

        private WorkflowHistoryEvents CreateActivityCompletedEventGraph()
        {
            var activityCompletedEventGraph = HistoryEventFactory.CreateActivityCompletedEventGraph(Identity.New("activity", "1.0"), "id", "result");
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
            var timerFailedEventGraph = HistoryEventFactory.CreateTimerFailedEventGraph(Identity.Timer("timer"), "cause");
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
            var combinedEventGraph = HistoryEventFactory.CreateTimerFailedEventGraph(Identity.Timer("timer"), "cause")
                .Concat(HistoryEventFactory.CreateTimerFiredEventGraph(Identity.Timer("timer"), TimeSpan.FromSeconds(4)));
            return new WorkflowHistoryEvents(combinedEventGraph);
        }
    }
}