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
    public class WorkflowStartedEventTests
    {
        [Test]
        public void Return_workflow_completed_decision_when_workflow_does_not_have_any_schedulable_items()
        {
            var emptyWorkflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(),Enumerable.Empty<HistoryEvent>());

            var startupDecisions = workflowEvent.Interpret(emptyWorkflow).GetDecisions();

            CollectionAssert.AreEqual(startupDecisions, new[] { new CompleteWorkflowDecision("Workflow completed as no schedulable item is found")});
        }

        [Test]
        public void Workflow_started_return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent(),Enumerable.Empty<HistoryEvent>())).GetDecisions();

            Assert.That(workflowStartedDecisions,Is.EquivalentTo(new []{new ScheduleActivityDecision("Download","1.0"), }));
        }

        [Test]
        public void Populate_properties_from_attributes()
        {
            var workflowStartedEvent = CreateWorkflowStartedEvent();
            var startAttributes = workflowStartedEvent.WorkflowExecutionStartedEventAttributes;

            var workflowEvent = new WorkflowStartedEvent(workflowStartedEvent,Enumerable.Empty<HistoryEvent>());

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
            Assert.AreEqual(startAttributes.TaskPriority, workflowEvent.TaskPriority);
            Assert.AreEqual(TimeSpan.FromSeconds(Convert.ToInt32(startAttributes.TaskStartToCloseTimeout)), workflowEvent.TaskStartToCloseTimeout);
        }

        [Test]
        public void Return_custom_workflow_startup_decision()
        {
            var customStartupAction = new Mock<WorkflowAction>();
            var workflow = new WorkflowWithCustomStartupAction(customStartupAction.Object);
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(), Enumerable.Empty<HistoryEvent>());

            var actualStartupAction = workflowEvent.Interpret(workflow);

            Assert.That(actualStartupAction,Is.EqualTo(customStartupAction.Object));
        }

        [Test]
        public void Return_workflow_started_action()
        {
            var workflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(),Enumerable.Empty<HistoryEvent>());

            var workflowAction = workflowEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.InstanceOf<WorkflowStartedAction>());
        }

        [Test]
        public void By_default_return_workflow_started_action()
        {
            var workflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent(), Enumerable.Empty<HistoryEvent>());

            var workflowAction = workflowEvent.Interpret(workflow);

            Assert.That(workflowAction, Is.EqualTo(new WorkflowStartedAction(workflow)));
        }

        private class EmptyWorkflow : Workflow
        {
        }

        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                AddActivity("Download", "1.0");

                AddActivity("Transcode", "2.0").DependsOn("Download", "1.0");
            }
        }

        private class WorkflowWithCustomStartupAction : Workflow
        {
            public WorkflowWithCustomStartupAction(WorkflowAction workflowAction)
            {
                OnStartup(s => workflowAction);
            }
        }

        private static HistoryEvent CreateWorkflowStartedEvent()
        {
            return new HistoryEvent()
            {
                WorkflowExecutionStartedEventAttributes = new WorkflowExecutionStartedEventAttributes()
                {
                    ChildPolicy = ChildPolicy.TERMINATE,
                    ContinuedExecutionRunId = "continue run id",
                    ExecutionStartToCloseTimeout = "100",
                    Input = "workflow input",
                    LambdaRole = "some role",
                    ParentInitiatedEventId = 10,
                    ParentWorkflowExecution = new WorkflowExecution() {RunId = "parent runid", WorkflowId = "parent workflow id"},
                    TagList = new List<string>() {"First", "Second"},
                    TaskList = new TaskList() {Name = "task name"},
                    TaskPriority = "1",
                    TaskStartToCloseTimeout = "30",
                }
            };
        }
    }
}