// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class RestartWorkflowActionTests
    {
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
        }
        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflowStartedEventGraph = _builder.WorkflowStartedEvent("input");
            var workflowStartedEvent = new WorkflowStartedEvent(workflowStartedEventGraph);
            var activityCompletedEvents = _builder.ActivityCompletedGraph(Identity.New("activityName", "1.0"), "id", "result");
            var activityCompletedEvent = new ActivityCompletedEvent(activityCompletedEvents.First(), activityCompletedEvents);
            var eventGraph = activityCompletedEvents.Concat(new[] {workflowStartedEventGraph});
            var workflowEvents = new WorkflowHistoryEvents(eventGraph);
            var workflow = new WorkflowToRestart();
            workflow.NewExecutionFor(workflowEvents);

            var restartWorkflowAction = (RestartWorkflowAction)activityCompletedEvent.Interpret(workflow);

            Assert.That(restartWorkflowAction.Input, Is.EqualTo(workflowStartedEvent.Input));
            Assert.That(restartWorkflowAction.ChildPolicy, Is.EqualTo(workflowStartedEvent.ChildPolicy));
            Assert.That(restartWorkflowAction.ExecutionStartToCloseTimeout, Is.EqualTo(workflowStartedEvent.ExecutionStartToCloseTimeout));
            Assert.That(restartWorkflowAction.TagList, Is.EqualTo(workflowStartedEvent.TagList));
            Assert.That(restartWorkflowAction.TaskList, Is.EqualTo(workflowStartedEvent.TaskList));
            Assert.That(restartWorkflowAction.TaskPriority, Is.EqualTo(workflowStartedEvent.TaskPriority));
            Assert.That(restartWorkflowAction.TaskStartToCloseTimeout, Is.EqualTo(workflowStartedEvent.TaskStartToCloseTimeout));
        }

        [WorkflowDescription("1.0")]
        private class WorkflowToRestart : Workflow
        {
            public WorkflowToRestart()
            {
                ScheduleActivity("activityName", "1.0").OnCompletion(e => RestartWorkflow());
            }
        }
    }
}