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
    public class WorkflowTasksTests
    {
        private Mock<IWorkflowClient> _workflowClient;
        [SetUp]
        public void Setup()
        {
            _workflowClient = new Mock<IWorkflowClient>();
        }

        [Test]
        public void Can_interpret_new_task_for_hosted_workflow()
        {
            var decisionTask = CreateDecisionTaskWithSignalEvents("token");
            var hostedWorkflows = new HostedWorkflows(new []{new TestWorkflow()});
            var workflowTasks = new WorkflowTasks(decisionTask,_workflowClient.Object);

            workflowTasks.ExecuteFor(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token");
        }

        [Test]
        public void Workflow_hisotry_events_can_not_be_queried_after_execution()
        {
            var decisionTask = CreateDecisionTaskWithSignalEvents("token");
            var hostedWorkflow = new TestWorkflow();
            var hostedWorkflows = new HostedWorkflows(new[] { hostedWorkflow });
            var workflowTasks = new WorkflowTasks(decisionTask, _workflowClient.Object);
            workflowTasks.ExecuteFor(hostedWorkflows);

            Assert.Throws<InvalidOperationException>(() => hostedWorkflow.AccessHistoryEvents());
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient(string token)
        {
            Func<IEnumerable<Decision>, bool> decisions = (d) =>
            {
                Assert.That(d.Count(), Is.EqualTo(1));
                var decision = d.First();
                Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.CompleteWorkflowExecution));
                return true;
            };
            _workflowClient.Verify(w=>w.RespondWithDecisions(token,It.Is<IEnumerable<Decision>>(d=>decisions(d))),Times.Once);
        }

        private DecisionTask CreateDecisionTaskWithSignalEvents(string token)
        {
            var historyEvent = HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input");
            return new DecisionTask()
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow", Version = "1.0" },
                Events = new List<HistoryEvent>(){ historyEvent},
                PreviousStartedEventId = historyEvent.EventId,
                StartedEventId = historyEvent.EventId,
                TaskToken = token
            }; 
        }

        [WorkflowDescription("1.0")]
        private class TestWorkflow:Workflow
        {
            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return CompleteWorkflow("result");
            }

            public void AccessHistoryEvents()
            {
                var active = IsActive;
            }
        }
    }
}