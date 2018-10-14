// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class SignalTests
    {
        private const string WorkflowName = "Workflow";
        private const string WorkflowVersion = "1.0";
        private const string SignalName = "name";
        private const string SignalInput = "input";
        private const string TimerName = "Timer";
        private Signal _signal;
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _builder;
        
        [SetUp]
        public void Setup()
        {
            _signal = new Signal(SignalName, SignalInput, new WorkflowItems());
            _eventGraphBuilder = new EventGraphBuilder();
            _builder = new HistoryEventsBuilder();
            _builder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent());
        }
        [Test]
        public void Invalid_arguments_test()
        {
            Assert.Throws<ArgumentException>(() => _signal.ForWorkflow(null, "runid"));
            Assert.Throws<ArgumentNullException>(() => _signal.ReplyTo(null));
        }
        [Test]
        public void Returns_signal_workflow_action()
        {
            var decisions = _signal.ForWorkflow("id", "runid").Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new SignalWorkflowDecision(SignalName, SignalInput, "id", "runid")}));
        }

        [Test]
        public void Replying_to_a_signal_returns_signal_workflow_workflow()
        {
            var receivedSignalEvent = new WorkflowSignaledEvent(_eventGraphBuilder.WorkflowSignaledEvent("someName","input1","rid","wid"));

            var decisions = _signal.ReplyTo(receivedSignalEvent).Decisions();

            Assert.That(decisions, Is.EqualTo(new []{new SignalWorkflowDecision(SignalName, SignalInput, "wid", "rid")}));
        }

        [Test]
        public void Signal_for_child_workflow()
        {
            var workflow = new SignalChildWorkflow();
            var identity = Identity.New(WorkflowName,WorkflowVersion);

            _builder.AddProcessedEvents(_eventGraphBuilder.ChildWorkflowStartedEventGraph(identity,"runid","input").ToArray());
            _builder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName),TimeSpan.Zero).ToArray());

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new SignalWorkflowDecision(SignalName, SignalInput, identity.Id, "runid") }));
        }

        [Test]
        public void Signal_for_child_workflow_using_generic_type_api()
        {
            var workflow = new SignalChildWorkflowUsingGenericTypeApi();
            var identity = Identity.New(WorkflowName, WorkflowVersion);

            _builder.AddProcessedEvents(_eventGraphBuilder.ChildWorkflowStartedEventGraph(identity, "runid", "input").ToArray());
            _builder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName), TimeSpan.Zero).ToArray());

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new SignalWorkflowDecision(SignalName, SignalInput, identity.Id, "runid") }));
        }

        [Test]
        public void Signal_for_child_workflow_using_child_workflow_query_api()
        {
            var workflow = new SignalChildWorkflowUsingQueryApi();
            var identity = Identity.New(WorkflowName, WorkflowVersion);

            _builder.AddProcessedEvents(_eventGraphBuilder.ChildWorkflowStartedEventGraph(identity, "runid", "input").ToArray());
            _builder.AddNewEvents(_eventGraphBuilder.TimerFiredGraph(Identity.Timer(TimerName), TimeSpan.Zero).ToArray());

            var decisions = workflow.Decisions(_builder.Result());

            Assert.That(decisions, Is.EqualTo(new[] { new SignalWorkflowDecision(SignalName, SignalInput, identity.Id, "runid") }));
        }

        private class SignalChildWorkflow : Workflow
        {
            public SignalChildWorkflow()
            {
                ScheduleChildWorkflow(WorkflowName, WorkflowVersion);
                ScheduleTimer(TimerName).OnFired(_ =>
                    Signal(SignalName, SignalInput).ForChildWorkflow(WorkflowName, WorkflowVersion));
            }
        }

        private class SignalChildWorkflowUsingGenericTypeApi : Workflow
        {
            public SignalChildWorkflowUsingGenericTypeApi()
            {
                ScheduleChildWorkflow<ChildWorkflow>();
                ScheduleTimer(TimerName).OnFired(_ =>
                    Signal(SignalName, SignalInput).ForChildWorkflow<ChildWorkflow>());
            }
        }

        private class SignalChildWorkflowUsingQueryApi : Workflow
        {
            public SignalChildWorkflowUsingQueryApi()
            {
                ScheduleChildWorkflow<ChildWorkflow>();
                ScheduleTimer(TimerName).OnFired(_ =>
                    Signal(SignalName, SignalInput).ForChildWorkflow(ChildWorkflow(WorkflowName, WorkflowVersion)));
            }
        }

        [WorkflowDescription(WorkflowVersion, Name = WorkflowName)]
        private class ChildWorkflow : Workflow
        {

        }
    }
}