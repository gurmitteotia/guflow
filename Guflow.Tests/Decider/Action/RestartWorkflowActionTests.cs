// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Linq;
using Amazon.SimpleWorkflow;
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
            var eventGraph = activityCompletedEvents.Concat(new[] {workflowStartedEventGraph});
            var workflowEvents = new WorkflowHistoryEvents(eventGraph, activityCompletedEvents.Last().EventId, activityCompletedEvents.First().EventId);
            var workflow = new WorkflowToRestart();

            var decisions = workflow.Decisions(workflowEvents);
            var decision = decisions.Single().SwfDecision();
            
            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.ContinueAsNewWorkflowExecution));
            var attr = decision.ContinueAsNewWorkflowExecutionDecisionAttributes;
            Assert.That(attr.Input, Is.EqualTo(workflowStartedEvent.Input));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo(workflowStartedEvent.ChildPolicy));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo(workflowStartedEvent.ExecutionStartToCloseTimeout.TotalSeconds.ToString()));
            Assert.That(attr.TagList, Is.EqualTo(workflowStartedEvent.TagList));
            Assert.That(attr.TaskList.Name, Is.EqualTo(workflowStartedEvent.TaskList));
            Assert.That(attr.TaskPriority, Is.EqualTo(workflowStartedEvent.TaskPriority.ToString()));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo(workflowStartedEvent.TaskStartToCloseTimeout.TotalSeconds.ToString()));
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