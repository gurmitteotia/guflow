// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

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
    public class ChildWorkflowItemTests
    {
        private EventGraphBuilder _builder;
        private Mock<IWorkflow> _workflow;
        private Identity _identity;
        private const string WorkflowName = "Workflow";
        private const string Version = "1.0";
        private const string PositionalName = "Pos";
        [SetUp]
        public void Setup()
        {
            _identity = Identity.New(WorkflowName, Version, PositionalName);
            _builder = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph("input")));
        }

        [Test]
        public void By_default_schedule_with_workflow_input()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);

            var decisions = item.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.StartChildWorkflowExecutionDecisionAttributes.Input , Is.EqualTo("input"));
        }

        [Test]
        public void Input_can_be_customized()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "\"input\"";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var item = new ChildWorkflowItem(_identity, workflow.Object);
            item.WithInput(_ => new{Id=1});

            var decisions = item.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.StartChildWorkflowExecutionDecisionAttributes.Input, Is.EqualTo("{\"Id\":1}"));
        }

        [Test]
        public void Schedule_the_child_workflow()
        {
            var description = new WorkflowDescription("1.0")
            {
                DefaultChildPolicy = "child",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(3),
                DefaultLambdaRole = "lambdarole",
                DefaultTaskListName = "task",
                DefaultTaskPriority = 1,
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(1),
            };

            var item = new ChildWorkflowItem(_identity, _workflow.Object, description);
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId , Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN , Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("child"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("3"));
            Assert.That(attr.LambdaRole, Is.EqualTo("lambdarole"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("task"));
            Assert.That(attr.TaskPriority, Is.EqualTo("1"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("1"));
            Assert.That(attr.TagList, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_workflow_without_providing_workflow_description()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy, Is.Null);
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.Null);
            Assert.That(attr.LambdaRole, Is.Null);
            Assert.That(attr.TaskList, Is.Null);
            Assert.That(attr.TaskPriority, Is.Null);
            Assert.That(attr.TaskStartToCloseTimeout, Is.Null);
            Assert.That(attr.TagList, Is.Empty);
        }

        [Test]
        public void Can_override_scheduling_properties_when_workflow_description_is_provided()
        {
            var description = new WorkflowDescription("1.0")
            {
                DefaultChildPolicy = "child",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(3),
                DefaultLambdaRole = "lambdarole",
                DefaultTaskListName = "task",
                DefaultTaskPriority = 1,
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(1),
            };

            var item = new ChildWorkflowItem(_identity, _workflow.Object, description);
            item.WithChildPolicy(_ => "newchild").WithLambdaRole(_ => "newlambda").OnTaskList(_ => "newtask")
                .WithPriority(_ => 2).WithTimeouts(_ => new WorkflowTimeouts()
                {
                    ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(4),
                    TaskStartToCloseTimeout = TimeSpan.FromSeconds(5)
                }).WithTags(_ => new []{"hello", "hi"});
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("newchild"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("4"));
            Assert.That(attr.LambdaRole, Is.EqualTo("newlambda"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("newtask"));
            Assert.That(attr.TaskPriority, Is.EqualTo("2"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("5"));
            Assert.That(attr.TagList, Is.EqualTo(new[]{"hello", "hi"}));
        }

        [Test]
        public void Can_override_scheduling_properties_when_workflow_description_is_not_provided()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);
            item.WithChildPolicy(_ => "newchild").WithLambdaRole(_ => "newlambda").OnTaskList(_ => "newtask")
                .WithPriority(_ => 2).WithTimeouts(_ => new WorkflowTimeouts()
                {
                    ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(4),
                    TaskStartToCloseTimeout = TimeSpan.FromSeconds(5)
                }).WithTags(_ => new[] { "hello", "hi" });
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("newchild"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("4"));
            Assert.That(attr.LambdaRole, Is.EqualTo("newlambda"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("newtask"));
            Assert.That(attr.TaskPriority, Is.EqualTo("2"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("5"));
            Assert.That(attr.TagList, Is.EqualTo(new[] { "hello", "hi" }));
        }

        [Test]
        public void All_events_can_return_child_workflow_completed_event()
        {
            var eventGraph = _builder.ChildWorkflowCompletedGraph(_identity, "runid", "input", "result").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new []
            {
                new ChildWorkflowCompletedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_failed_event()
        {
            var eventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "details").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new[]
            {
                new ChildWorkflowFailedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_cancelled_event()
        {
            var eventGraph = _builder.ChildWorkflowCancelledEventGraph(_identity, "runid", "input", "details").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowCancelledEvent(eventGraph.First(), eventGraph),
                new ExternalWorkflowCancellationRequestedEvent(eventGraph.Skip(1).First()), 
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_timedout_event()
        {
            var eventGraph = _builder.ChildWorkflowTimedoutEventGraph(_identity, "runid", "input", "details").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new[]
            {
                new ChildWorkflowTimedoutEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_started_event()
        {
            var eventGraph = _builder.ChildWorkflowStartedEventGraph(_identity, "runid", "input").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new[]
            {
                new ChildWorkflowStartedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_start_failed_event()
        {
            var eventGraph = _builder.ChildWorkflowStartFailedEventGraph(_identity, "runid", "input").ToArray();
            var childWorkflow = ChildWorkflow(eventGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new[]
            {
                new ChildWorkflowStartFailedEvent(eventGraph.First(), eventGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_failed_and_completed_event()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input","reason","detail").ToArray();
            var completedEventGraph = _builder.ChildWorkflowCompletedGraph(_identity, "runid", "input","result").ToArray();
            var allEventsGraph = completedEventGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new ChildWorkflowEvent[]
            {
                new ChildWorkflowCompletedEvent(completedEventGraph.First(), allEventsGraph),
                new ChildWorkflowFailedEvent(failedEventGraph.First(), allEventsGraph)
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_failed_and_started_event()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var startedEventGraph = _builder.ChildWorkflowStartedEventGraph(_identity, "runid", "input").ToArray();
            var allEventsGraph = startedEventGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new ChildWorkflowEvent[]
            {
                new ChildWorkflowStartedEvent(startedEventGraph.First(), allEventsGraph),
                new ChildWorkflowFailedEvent(failedEventGraph.First(), allEventsGraph)
            }));
        }

        [Test]
        public void All_events_can_return_external_workflow_cancellation_requested_event_and_workflow_started_event()
        {
            var cancellationRequested = _builder.ChildWorkflowCancellationRequestedEventGraph(_identity, "runid", "input").ToArray();
            
            var childWorkflow = ChildWorkflow(cancellationRequested);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ExternalWorkflowCancellationRequestedEvent(cancellationRequested.First()), 
                new ChildWorkflowStartedEvent(cancellationRequested.Skip(2).First(), cancellationRequested), 
            }));
        }

        [Test]
        public void All_events_can_return_child_workflow_cancelled_event__and_external_workflow_cancellation_requested_event()
        {
            var cancelledEvent = _builder.ChildWorkflowCancelledEventGraph(_identity, "runid", "input", "details").ToArray();

            var childWorkflow = ChildWorkflow(cancelledEvent);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowCancelledEvent(cancelledEvent.First(), cancelledEvent),
                new ExternalWorkflowCancellationRequestedEvent(cancelledEvent.Skip(1).First()),
            }));
        }

        [Test]
        public void All_events_can_return_reschedule_timer_events()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var timerStartedGraph = _builder.TimerStartedGraph(_identity, TimeSpan.FromSeconds(20), true).ToArray();
            var allEventsGraph = timerStartedGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var allEvents = childWorkflow.AllEvents(true);

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new TimerStartedEvent(timerStartedGraph.First(), allEventsGraph),
                new ChildWorkflowFailedEvent(failedEventGraph.First(), allEventsGraph)
            }));
        }

        [Test]
        public void All_events_can_filter_out_reschedule_timer_events()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var timerStartedGraph = _builder.TimerStartedGraph(_identity, TimeSpan.FromSeconds(20), true).ToArray();
            var allEventsGraph = timerStartedGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var allEvents = childWorkflow.AllEvents();

            Assert.That(allEvents, Is.EqualTo(new WorkflowItemEvent[]
            {
                new ChildWorkflowFailedEvent(failedEventGraph.First(), allEventsGraph)
            }));
        }


        [Test]
        public void Last_event_can_return_child_workflow_completed_event()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var completedEvent = _builder.ChildWorkflowCompletedGraph(_identity, "runid", "input", "result").ToArray();
            var allEventsGraph = completedEvent.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var lastEvent = childWorkflow.LastEvent();

            Assert.That(lastEvent, Is.EqualTo(new ChildWorkflowCompletedEvent(completedEvent.First(), allEventsGraph)));
        }

        [Test]
        public void Last_event_is_cached()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var completedEvent = _builder.ChildWorkflowCompletedGraph(_identity, "runid", "input", "result").ToArray();
            var allEventsGraph = completedEvent.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var lastEvent1 = childWorkflow.LastEvent();
            var lastEvent2 = childWorkflow.LastEvent();

            Assert.That(ReferenceEquals(lastEvent1, lastEvent2));
        }


        [Test]
        public void Last_event_can_return_reschedule_timer_event()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var timerStartedGraph = _builder.TimerStartedGraph(_identity, TimeSpan.FromSeconds(20), true).ToArray();
            var allEventsGraph = timerStartedGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var lastEvent = childWorkflow.LastEvent(true);

            Assert.That(lastEvent, Is.EqualTo(new TimerStartedEvent(timerStartedGraph.First(), allEventsGraph)));
        }

        [Test]
        public void Last_event_can_filter_out_reschedule_timer_event()
        {
            var failedEventGraph = _builder.ChildWorkflowFailedEventGraph(_identity, "runid", "input", "reason", "detail").ToArray();
            var timerStartedGraph = _builder.TimerStartedGraph(_identity, TimeSpan.FromSeconds(20), true).ToArray();
            var allEventsGraph = timerStartedGraph.Concat(failedEventGraph);
            var childWorkflow = ChildWorkflow(allEventsGraph);

            var lastEvent = childWorkflow.LastEvent();

            Assert.That(lastEvent, Is.EqualTo(new ChildWorkflowFailedEvent(failedEventGraph.First(), allEventsGraph)));
        }

        [Test]
        public void Reschedule_decision_is_timer_schedule_decision_for_child_workflow_item()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);
            var decisions = item.RescheduleDecisions(TimeSpan.FromSeconds(20));

            Assert.That(decisions, Is.EqualTo(new[]{new ScheduleTimerDecision(_identity, TimeSpan.FromSeconds(20), true)}));
        }

        [Test]
        public void Invalid_arguments()
        {
            var childWorkflowItem = new ChildWorkflowItem(_identity, Mock.Of<IWorkflow>());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCancelled(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTerminated(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTimedout(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnStartFailed(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithInput(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithChildPolicy(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithLambdaRole(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTaskList(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithPriority(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithTimeouts(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithTags(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.When(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.When(null,_=>WorkflowAction.Empty));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.When(_=>true, null));

            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterTimer(null));
            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterActivity(null, "v"));
            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterActivity("n", null));
            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterLambda(null));
            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterChildWorkflow(null,"1.0"));
            Assert.Throws<ArgumentException>(() => childWorkflowItem.AfterChildWorkflow("n", null));

        }

        private ChildWorkflowItem ChildWorkflow(IEnumerable<HistoryEvent> events)
        {
            _workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(events));
            return new ChildWorkflowItem(_identity, _workflow.Object);
        }
    }
}