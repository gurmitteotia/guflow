﻿using System;
using System.Linq;
using Guflow.Tests.TestWorkflows;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class TimerCancelledEventTests
    {
        private TimerCancelledEvent _timerCancelledEvent;
        private const string _timerName ="timer name";
        private readonly TimeSpan _fireAfter = TimeSpan.FromSeconds(2);
        [SetUp]
        public void Setup()
        {
            _timerCancelledEvent = CreateTimerCancelledEvent(Identity.Timer(_timerName),_fireAfter);
        }

        [Test]
        public void Should_populate_properties_from_event_attributes()
        {
            Assert.That(_timerCancelledEvent.Name,Is.EqualTo(_timerName));
            Assert.That(_timerCancelledEvent.FiredAfter,Is.EqualTo(_fireAfter));
        }

        [Test]
        public void Throws_exception_when_timer_is_not_found()
        {
            var workflow = new EmptyWorkflow();
            Assert.Throws<IncompatibleWorkflowException>(() => _timerCancelledEvent.Interpret(workflow));
        }

        [Test]
        public void By_default_return_cancel_workflow_action()
        {
            var workflow = new WorkflowWithTimer();

            var workflowAction = _timerCancelledEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(WorkflowAction.CancelWorkflow("TIMER_CANCELLED")));
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var customAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomAction(customAction.Object);

            var workflowAction = _timerCancelledEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(customAction.Object));
        }

        [Test]
        public void Throws_exception_when_rescheduled_timer_is_not_found()
        {
            var workflow = new SingleActivityWorkflow();
            var timerCancelledEvent = CreateRescheduledTimerCancelledEvent(Identity.New("DifferntName", SingleActivityWorkflow.ActivityVersion), _fireAfter);
            Assert.Throws<IncompatibleWorkflowException>(() => timerCancelledEvent.Interpret(workflow));
        }

        [Test]
        public void By_default_return_cancel_workflow_action_for_rescheduled_timer()
        {
            var workflow = new SingleActivityWorkflow();
            var timerCancelledEvent = CreateRescheduledTimerCancelledEvent(Identity.New(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion),_fireAfter);

            var workflowAction = timerCancelledEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(WorkflowAction.CancelWorkflow("TIMER_CANCELLED")));
        }

        [Test]
        public void Can_return_custom_workflow_action_for_rescheduled_timer()
        {
            var customAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomTimerAction(customAction.Object);
            var timerCancelledEvent = CreateRescheduledTimerCancelledEvent(Identity.New(SingleActivityWorkflow.ActivityName, SingleActivityWorkflow.ActivityVersion), _fireAfter);

            var workflowAction = timerCancelledEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(customAction.Object));
        }

        private TimerCancelledEvent CreateTimerCancelledEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerCancelledEventGraph = HistoryEventFactory.CreateTimerCancelledEventGraph(identity, fireAfter);
            return new TimerCancelledEvent(timerCancelledEventGraph.First(),timerCancelledEventGraph);
        }
        private TimerCancelledEvent CreateRescheduledTimerCancelledEvent(Identity identity, TimeSpan fireAfter)
        {
            var timerCancelledEventGraph = HistoryEventFactory.CreateTimerCancelledEventGraph(identity, fireAfter,true);
            return new TimerCancelledEvent(timerCancelledEventGraph.First(), timerCancelledEventGraph);
        }

        private class WorkflowWithTimer : Workflow
        {
            public WorkflowWithTimer()
            {
                AddTimer(_timerName);
            }
        }
        private class WorkflowWithCustomAction : Workflow
        {
            public WorkflowWithCustomAction(WorkflowAction workflowAction)
            {
                AddTimer(_timerName).OnCancelled(c => workflowAction);
            }
        }
        private class SingleActivityWorkflow : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public SingleActivityWorkflow()
            {
                AddActivity(ActivityName, ActivityVersion);
            }
        }
        private class WorkflowWithCustomTimerAction : Workflow
        {
            public const string ActivityName = "Download";
            public const string ActivityVersion = "1.0";
            public WorkflowWithCustomTimerAction(WorkflowAction workflowAction)
            {
                AddActivity(ActivityName, ActivityVersion).OnTimerCancelled(c => workflowAction);
            }
        }
    }
}