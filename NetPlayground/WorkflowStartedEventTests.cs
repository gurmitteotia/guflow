using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using NUnit.Framework;

namespace NetPlayground
{
    [TestFixture]
    public class WorkflowStartedEventTests
    {
        [Test]
        public void Return_workflow_completed_decision_when_workflow_does_not_have_any_schedulable_items()
        {
            var emptyWorkflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent());

            var startupDecisions = workflowEvent.Interpret(emptyWorkflow).GetDecisions();

            Assert.That(startupDecisions.Count(),Is.EqualTo(1));
            startupDecisions.AssertThatWorkflowIsCompleted("Workflow completed as no schedulable item is found");
        }

        [Test]
        public void Workflow_started_return_schedule_decisions_for_startup_items()
        {
            var workflow = new TestWorkflow();

            var workflowStartedDecisions = workflow.WorkflowStarted(new WorkflowStartedEvent(new HistoryEvent())).GetDecisions();

            Assert.That(workflowStartedDecisions.Count(), Is.EqualTo(1));
            workflowStartedDecisions.AssertThatActivityIsScheduled("Download", "1.0");
        }

        [Test]
        public void Populate_properties_from_attributes()
        {
            var workflowStartedEvent = CreateWorkflowStartedEvent();
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
            Assert.AreEqual(startAttributes.TaskPriority, workflowEvent.TaskPriority);
            Assert.AreEqual(TimeSpan.FromSeconds(Convert.ToInt32(startAttributes.TaskStartToCloseTimeout)), workflowEvent.TaskStartToCloseTimeout);
        }

        [Test]
        public void Return_workflow_started_action()
        {
            var workflow = new EmptyWorkflow();
            var workflowEvent = new WorkflowStartedEvent(new HistoryEvent());

            var workflowAction = workflowEvent.Interpret(workflow);

            Assert.That(workflowAction,Is.InstanceOf<WorkflowStartedAction>());
        }


        //[Test]
        //public void Return_workflow_start_up_decision()
        //{
        //    var workflowAction = new TestWorkflowAction();
        //    WorkflowReturnThisStartupDecision(workflowAction);
        //    var workflowEvent = new WorkflowStartedEvent(new HistoryEvent());

        //    var decision = workflowEvent.Interpret(_workflow.Object);

        //    Assert.That(decision, Is.EqualTo(workflowAction));
        //}
      
        //private void WorkflowReturnThisStartupDecision(WorkflowAction workflowAction)
        //{
        //    _workflow.Setup(w => w.WorkflowStarted(It.IsAny<WorkflowStartedEvent>())).Returns(workflowAction);
        //}

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

        private HistoryEvent CreateWorkflowStartedEvent()
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