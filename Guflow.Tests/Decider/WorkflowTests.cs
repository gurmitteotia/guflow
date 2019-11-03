// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowTests
    {
        private Mock<IWorkflowHistoryEvents> _workflowEvents;
        private EventGraphBuilder _graphBuilder;
        private HistoryEventsBuilder _builder;
        private Workflow _workflow;

        [SetUp]
        public void Setup()
        {
            _graphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _workflowEvents = new Mock<IWorkflowHistoryEvents>();
            _workflow = new StubWorkflow();
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
        public void Get_activity_by_its_event()
        {
            var identity = Identity.New("Activity1", "1.0");
            var eventGraph = _graphBuilder.ActivityCompletedGraph(identity.ScheduleId(), "id", "result");
            var activityCompletedEvent = new ActivityCompletedEvent(eventGraph.First(), eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetActivityOf(activityCompletedEvent);

            Assert.That(activity, Is.EqualTo(new ActivityItem(identity, workflow)));
        }

        [Test]
        public void Get_timer_by_its_event()
        {
            var identity = Identity.Timer("Timer1");
            var eventGraph = _graphBuilder.TimerFiredGraph(identity.ScheduleId(), TimeSpan.FromSeconds(2));
            _builder.AddNewEvents(eventGraph.ToArray());

            var workflow = new TestWorkflow();
            workflow.Decisions(_builder.Result());

            var timer = workflow.TimerItem;

            Assert.That(workflow.TimerItem.Name, Is.EqualTo("Timer1"));
        }

        [Test]
        public void Get_lambda_item_form_its_event()
        {
            var identity = Identity.Lambda("Lambda");
            var eventGraph = _graphBuilder.LambdaCompletedEventGraph(identity.ScheduleId(), "id", "result");
            var @event = new LambdaCompletedEvent(eventGraph.First(), eventGraph);
            var workflow = new TestWorkflow();

            var activity = workflow.GetLambda(@event);

            Assert.That(activity, Is.EqualTo(new LambdaItem(identity, workflow)));
        }
        [Test]
        public void Invalid_argument_tests()
        {
            Assert.Throws<ArgumentException>(() => new WithNullActivityName());
            Assert.Throws<ArgumentException>(() => new WithNullActivityVersion());
            Assert.Throws<ArgumentException>(() => new WithNullTimerName());
        }
        [Test]
        public void Returns_complete_workflow_decision_when_only_propose_to_complete_workflow_decision_is_generated()
        {
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(new CompleteWorkflowDecision("complete", true)));
            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Filters_out_propose_to_complete_workflow_decision_when_it_is_generated_along_with_other_decisions()
        {
            var decisions = new WorkflowDecision[]{new CompleteWorkflowDecision("complete", true),
                                                                                new ScheduleActivityDecision(Identity.New("something","1.0").ScheduleId())};
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new ScheduleActivityDecision(Identity.New("something", "1.0").ScheduleId()) }));
        }

        [Test]
        public void Filters_out_propose_to_complete_workflow_decision_and_any_scheduling_decision_when_it_is_generated_along_with_complete_workflow_decision()
        {
            var decisions = AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete", true),
                                                                                new CompleteWorkflowDecision("complete3")});
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete3") }));
        }

        [Test]
        public void Filters_out_scheduling_decisions_when_generated_along_complete_workflow_decision()
        {
            var decisions = AllNonCompletingDecisions().Concat(new[] { new CompleteWorkflowDecision("complete") });
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Filters_out_activity_scheduling_decisions_when_generated_along_with_fail_workflow_decision()
        {
            var decisions = AllNonCompletingDecisions().Concat(new[] { new FailWorkflowDecision("reason", "detail") });
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Filters_out_activity_scheduling_decisions_when_generated_along_with_cancel_workflow_decision()
        {
            var decisions = AllNonCompletingDecisions().Concat(new[] { new CancelWorkflowDecision("detail") });
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Returns_fail_workflow_decision_when_multiple_close_workflow_decisions_are_generated()
        {
            var decisions = new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
                new CompleteWorkflowDecision("result2",true),
                new FailWorkflowDecision("reason","detail"),
            };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new FailWorkflowDecision("reason", "detail") }));
        }

        [Test]
        public void Returns_cancel_workflow_decision_when_it_generated_along_with_complete_workflow_decision()
        {
            var decisions = new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CancelWorkflowDecision("detail"),
            };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CancelWorkflowDecision("detail") }));
        }

        [Test]
        public void Return_complete_workflow_decision_when_it_generated_along_with_a_proposed_complete_workflow_decision()
        {
            var decisions = new WorkflowDecision[]
            {
                new CompleteWorkflowDecision("result",false),
                new CompleteWorkflowDecision("result2",true),
            };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("result") }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_completion()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            var decisions = new WorkflowDecision[] { new CompleteWorkflowDecision("result2", true) };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions.ToArray()));

            var workflowDecisions = workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_failure()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);
            var decisions = new[] { new FailWorkflowDecision("reason", "detail") };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions));

            var workflowDecisions = workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }

        [Test]
        public void Workflow_can_return_custom_decisions_during_cancellation()
        {
            var workflowDecision = new Mock<WorkflowDecision>(false, false);
            var workflow = new WorkflowToReturnCustomActionOnClosing(workflowDecision.Object);

            var decisions = new[] { new CancelWorkflowDecision("detail") };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions));

            var workflowDecisions = workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { workflowDecision.Object }));
        }
        [Test]
        public void Ignores_when_null_workflow_action_is_returned_during_completion()
        {
            var workflow = new WorkflowToReturnNullActionOnClosing();
            var decisions = new[] { new CompleteWorkflowDecision("result2", true) };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions));

            var workflowDecisions = workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }
        [Test]
        public void Returns_empty_decisions_when_only_propose_to_complete_workflow_decision_is_generated_and_workflow_is_active()
        {
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(new CompleteWorkflowDecision("complete", true)));
            _workflowEvents.Setup(h => h.HasActiveEvent()).Returns(true);

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.Empty);
        }

        [Test]
        public void Filters_out_empty_workflow_decisions()
        {
            var decisions = new[] { new CompleteWorkflowDecision("complete"), WorkflowDecision.Empty };
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Events(decisions));

            var workflowDecisions = _workflow.Decisions(_workflowEvents.Object);

            Assert.That(workflowDecisions, Is.EqualTo(new[] { new CompleteWorkflowDecision("complete") }));
        }

        [Test]
        public void Throws_exception_when_accessing_the_history_events_afer_execution()
        {
            _workflowEvents.Setup(w => w.NewEvents()).Returns(Enumerable.Empty<WorkflowEvent>());
            var workflow = new WorkflowToAccesHistoryEvents();
            workflow.Decisions(_workflowEvents.Object);
            Assert.Throws<InvalidOperationException>(() => workflow.AccessEvents());
        }

        [Test]
        public void All_lambdas()
        {
            var workflow = new WorkflowWithLambda();

            Assert.That(workflow.AllLambdaItems.Count(), Is.EqualTo(2));
            Assert.That(workflow.LambdaItem("Lambda2").Name, Is.EqualTo("Lambda2"));
        }

        [Test]
        public void Read_workflow_id_and_runid()
        {
            _builder.AddNewEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddWorkflowRunId("runid");
            _builder.AddWorkflowId("wid");
            var w = new WorkflowToReadIds();
            w.Decisions(_builder.Result());

            Assert.That(w.WorkflowId, Is.EqualTo("wid"));
            Assert.That(w.WorkflowRunId, Is.EqualTo("runid"));
        }

        [Test]
        public void Signal_IsReceived_is_true()
        {
            _builder.AddNewEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("S1", "input"));
            var w = new WorkflowIsSignalReceivedAPI("s1");
            w.Decisions(_builder.Result());

            Assert.That(w.IsSignalReceived, Is.True);
        }

        [Test]
        public void Signal_IsReceived_is_false()
        {
            _builder.AddNewEvents(_graphBuilder.WorkflowStartedEvent());
            _builder.AddNewEvents(_graphBuilder.WorkflowSignaledEvent("S1", "input"));
            var w = new WorkflowIsSignalReceivedAPI("s2");
            w.Decisions(_builder.Result());

            Assert.That(w.IsSignalReceived, Is.False);
        }
        [Test]
        public void Workflow_execution_order()
        {
            _builder.AddNewEvents(_graphBuilder.WorkflowStartedEvent());
            var w = new WorkflowExecutionOrder();
            w.Decisions(_builder.Result());

            Assert.That(w.Orders, Is.EqualTo(new []{"BeforeExecution", "Execution", "AfterExecution"}));
        }

        private IEnumerable<WorkflowEvent> Events(params WorkflowDecision[] decisions)
            => decisions.Select(d => new TestEvent(d));


        private IEnumerable<WorkflowDecision> AllNonCompletingDecisions()
        {
            return new WorkflowDecision[]
            {
                new ScheduleActivityDecision(Identity.New("id", "1.0").ScheduleId()),
                ScheduleTimerDecision.WorkflowItem(Identity.Timer("timer").ScheduleId(), TimeSpan.FromSeconds(2)),
                new CancelActivityDecision(Identity.New("newid", "1.0").ScheduleId()),
                new CancelTimerDecision(Identity.Timer("first").ScheduleId()),
                new ScheduleLambdaDecision(Identity.Lambda("name").ScheduleId(),"input" ),
                new ScheduleChildWorkflowDecision(Identity.New("w","v").ScheduleId(), "input"),
            };
        }

        private class WorkflowWithLambda : Workflow
        {
            public WorkflowWithLambda()
            {
                ScheduleLambda("Lambda1");
                ScheduleLambda("Lambda2");
            }

            public IEnumerable<ILambdaItem> AllLambdaItems => Lambdas;

            public ILambdaItem LambdaItem(string name) => Lambda(name);
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
                _customAction.Setup(w => w.Decisions(It.IsAny<IWorkflow>())).Returns(new[] { workflowDecision });
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

        private class TestEvent : WorkflowEvent
        {
            private readonly WorkflowDecision _decision;

            public TestEvent(WorkflowDecision decision) 
                : base(new HistoryEvent(){EventId = 0, EventTimestamp = DateTime.UtcNow})
            {
                _decision = decision;
            }

            internal override WorkflowAction Interpret(IWorkflow workflow)
            {
                return WorkflowAction.Custom(_decision);
            }
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
                ScheduleActivity("Download", "1.0").AfterActivity("Download", "1.0");
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
                ScheduleActivity("_timerName", "version").AfterActivity("ParentName", "parentVer");
            }
        }
        private class WorkflowWithNonExistentParentTimerItem : Workflow
        {
            public WorkflowWithNonExistentParentTimerItem()
            {
                ScheduleActivity("_timerName", "version").AfterTimer("ParentName");
            }
        }
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleTimer("Timer1").OnFired(e =>
                {
                    TimerItem = Timer(e);
                    return Ignore;
                });
                ScheduleActivity("Activity1", "1.0");
                ScheduleLambda("Lambda");
                ScheduleChildWorkflow("Workflow", "1.0");
            }
            public IActivityItem GetActivityOf(WorkflowItemEvent workflowItemEvent)
            {
                return Activity(workflowItemEvent);
            }
            public ITimerItem GetTimerOf(WorkflowItemEvent workflowItemEvent)
            {
                return Timer(workflowItemEvent);
            }

            public ILambdaItem GetLambda(WorkflowItemEvent @event)
            {
                return Lambda(@event);
            }

            public IChildWorkflowItem GetChildWorkflow(WorkflowItemEvent @event)
            {
                return ChildWorkflow(@event);
            }

            public ITimerItem TimerItem;
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

        private class WorkflowToAccesHistoryEvents : Workflow
        {
            public bool AccessEvents() => HasActiveEvent;
        }

        private class WorkflowToReadIds : Workflow
        {
            [WorkflowEvent(EventName.WorkflowStarted)]
            public WorkflowAction Started()
            {
                WorkflowId = Id;
                WorkflowRunId = RunId;
                return Ignore;
            }

            public string WorkflowId;
            public string WorkflowRunId;
        }

        private class WorkflowIsSignalReceivedAPI : Workflow
        {
            private readonly string _signalName;
            public WorkflowIsSignalReceivedAPI(string signalName)
            {
                _signalName = signalName;
            }
            [WorkflowEvent(EventName.WorkflowStarted)]
            public WorkflowAction Started()
            {
                IsSignalReceived = Signal(_signalName).IsReceived();
                return Ignore;
            }

            public bool IsSignalReceived;
        }

        private class WorkflowExecutionOrder : Workflow
        {
            public List<string> Orders = new List<string>();

            [WorkflowEvent(EventName.WorkflowStarted)]
            public WorkflowAction OnStarted()
            {
                Orders.Add("Execution");
                return Ignore;
            }

            protected override void BeforeExecution()
            {
                Orders.Add("BeforeExecution");
            }

            protected override void AfterExecution()
            {
                Orders.Add("AfterExecution");
            }
        }
    }

}