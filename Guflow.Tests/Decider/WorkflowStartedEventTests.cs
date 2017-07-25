﻿using System;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowStartedEventTests
    {
        [Test]
        public void Populate_properties_from_event_attributes()
        {
            var workflowStartedEvent = HistoryEventFactory.CreateWorkflowStartedEvent();
            var startAttributes = workflowStartedEvent.WorkflowExecutionStartedEventAttributes;

            var workflowEvent = new WorkflowStartedEvent(workflowStartedEvent);

            Assert.AreEqual(startAttributes.ChildPolicy.Value, workflowEvent.ChildPolicy);
            Assert.AreEqual(startAttributes.ContinuedExecutionRunId, workflowEvent.ContinuedExecutionRunId);
            Assert.AreEqual(TimeSpan.FromSeconds(Convert.ToInt32(startAttributes.ExecutionStartToCloseTimeout)), workflowEvent.ExecutionStartToCloseTimeout);
            Assert.AreEqual(startAttributes.Input, workflowEvent.Input);
            Assert.AreEqual(startAttributes.LambdaRole, workflowEvent.LambdaRole);
            Assert.AreEqual(startAttributes.ParentInitiatedEventId, workflowEvent.ParentInitiatedEventId);
            Assert.AreEqual(startAttributes.ParentWorkflowExecution.RunId, workflowEvent.ParentWorkflowRunId);
            Assert.AreEqual(startAttributes.ParentWorkflowExecution.WorkflowId, workflowEvent.ParentWorkflowId);
            Assert.AreEqual(startAttributes.TagList, workflowEvent.TagList);
            Assert.AreEqual(startAttributes.TaskList.Name, workflowEvent.TaskList);
            Assert.AreEqual(int.Parse(startAttributes.TaskPriority), workflowEvent.TaskPriority);
            Assert.AreEqual(TimeSpan.FromSeconds(Convert.ToInt32(startAttributes.TaskStartToCloseTimeout)), workflowEvent.TaskStartToCloseTimeout);
        }

        [Test]
        public void Can_return_custom_workflow_action()
        {
            var customStartupAction = new Mock<WorkflowAction>().Object;
            var workflow = new WorkflowWithCustomStartupAction(customStartupAction);
            var workflowEvent = new WorkflowStartedEvent(HistoryEventFactory.CreateWorkflowStartedEvent());

            var actualStartupAction = workflowEvent.Interpret(workflow);

            Assert.That(actualStartupAction,Is.EqualTo(customStartupAction));
        }

        [Test]
        public void Return_start_workflow_action()
        {
            var workflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(HistoryEventFactory.CreateWorkflowStartedEvent());

            var workflowAction = workflowEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.EqualTo(workflow.StartupAction));
        }

        [Test]
        public void Raise_workflow_started_event()
        {
            var workflow = new EmptyWorkflow();
            WorkflowStartedEventArgs eventArgs = null;
            workflow.Started += (s, e) => { eventArgs = e; };
            var workflowEvent = new WorkflowStartedEvent(HistoryEventFactory.CreateWorkflowStartedEvent());

            workflowEvent.Interpret(workflow);

            Assert.That(eventArgs,Is.Not.Null);
            Assert.That(eventArgs.StartEvent, Is.EqualTo(workflowEvent));
        }

        private class EmptyWorkflow : Workflow
        {
        }
        private class WorkflowWithCustomStartupAction : Workflow
        {
            private readonly WorkflowAction _workflowAction;

            public WorkflowWithCustomStartupAction(WorkflowAction workflowAction)
            {
                _workflowAction = workflowAction;
            }

            [WorkflowEvent(EventName.WorkflowStarted)]
            protected WorkflowAction OnStart(WorkflowStartedEvent workflowSartedEvent)
            {
                return _workflowAction;
            }
        }
    }
}