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
        private EventGraphBuilder _eventGraphBuilder;
        private HistoryEventsBuilder _eventsBuilder;

        [SetUp]
        public void Setup()
        {
            _eventGraphBuilder = new EventGraphBuilder();
            _eventsBuilder = new HistoryEventsBuilder();
        }
        [Test]
        public void Can_be_returned_as_custom_action()
        {
            var workflowStartedEventGraph = _eventGraphBuilder.WorkflowStartedEvent("input");
            var workflowStartedEvent = new WorkflowStartedEvent(workflowStartedEventGraph);
            _eventsBuilder.AddProcessedEvents(workflowStartedEventGraph);
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ActivityCompletedGraph(Identity.New("activityName", "1.0").ScheduleId(), "id", "result").ToArray());

            var workflow = new WorkflowToRestart();

            var decisions = workflow.Decisions(_eventsBuilder.Result());
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

        [Test]
        public void Override_start_properties_when_restarting()
        {
            _eventsBuilder.AddProcessedEvents(_eventGraphBuilder.WorkflowStartedEvent("input"));
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ActivityCompletedGraph(Identity.New("activityName", "1.0").ScheduleId(), "id", "result").ToArray());

            var workflow = new WorkflowToRestartWithCustomProperties();

            var decisions = workflow.Decisions(_eventsBuilder.Result());
            var decision = decisions.Single().SwfDecision();

            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.ContinueAsNewWorkflowExecution));
            Assert.That(decision.ContinueAsNewWorkflowExecutionDecisionAttributes.LambdaRole, Is.EqualTo("new lambda role"));
        }


        [Test]
        public void Restart_using_default_properties()
        {
            _eventsBuilder.AddNewEvents(_eventGraphBuilder
                .ActivityCompletedGraph(Identity.New("activityName", "1.0").ScheduleId(), "id", "result").ToArray());

            var workflow = new WorkflowToRestartWithDefault();

            var decisions = workflow.Decisions(_eventsBuilder.Result());
            var decision = decisions.Single().SwfDecision();

            Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.ContinueAsNewWorkflowExecution));
            var attr = decision.ContinueAsNewWorkflowExecutionDecisionAttributes;
            Assert.That(attr.LambdaRole, Is.Null);
            Assert.That(attr.TaskList, Is.Null);
            Assert.That(attr.ChildPolicy, Is.Null);
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.Null);
            Assert.That(attr.Input, Is.Null);
            Assert.That(attr.TagList, Is.Empty);
            Assert.That(attr.TaskPriority, Is.Null);
            Assert.That(attr.TaskStartToCloseTimeout, Is.Null);
            Assert.That(attr.WorkflowTypeVersion, Is.Null);
        }

        [WorkflowDescription("1.0")]
        private class WorkflowToRestart : Workflow
        {
            public WorkflowToRestart()
            {
                ScheduleActivity("activityName", "1.0").OnCompletion(e => RestartWorkflow());
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowToRestartWithCustomProperties : Workflow
        {
            public WorkflowToRestartWithCustomProperties()
            {
                ScheduleActivity("activityName", "1.0").OnCompletion(e =>
                {
                    var action = RestartWorkflow();
                    action.DefaultLambdaRole = "new lambda role";
                    return action;
                });
            }
        }

        [WorkflowDescription("1.0")]
        private class WorkflowToRestartWithDefault : Workflow
        {
            public WorkflowToRestartWithDefault()
            {
                ScheduleActivity("activityName", "1.0").OnCompletion(e => RestartWorkflow(true));
            }
        }
    }
}