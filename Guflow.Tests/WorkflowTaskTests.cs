using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests
{
    [TestFixture]
    public class WorkflowTaskTests
    {
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflowClient;
        [SetUp]
        public void Setup()
        {
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
        }

        [Test]
        public void Can_interpret_new_task_for_hosted_workflow()
        {
            var decisionTask = CreateDecisionTaskWithSignalEvents("token");
            var hostedWorkflows = new HostedWorkflows(new []{new TestWorkflow()});
            var workflowTasks = WorkflowTask.CreateFor(decisionTask);

            workflowTasks.ExecuteFor(hostedWorkflows, _amazonWorkflowClient.Object);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token");
        }

        [Test]
        public void Workflow_hisotry_events_can_not_be_queried_after_execution()
        {
            var decisionTask = CreateDecisionTaskWithSignalEvents("token");
            var hostedWorkflow = new TestWorkflow();
            var hostedWorkflows = new HostedWorkflows(new[] { hostedWorkflow });
            var workflowTasks = WorkflowTask.CreateFor(decisionTask);
            workflowTasks.ExecuteFor(hostedWorkflows, _amazonWorkflowClient.Object);

            Assert.Throws<InvalidOperationException>(() => hostedWorkflow.AccessHistoryEvents());
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient(string token)
        {
            Func<RespondDecisionTaskCompletedRequest, bool> decisions = (r) =>
            {
                Assert.That(r.TaskToken,Is.EqualTo(token));
                var d = r.Decisions;
                Assert.That(d.Count(), Is.EqualTo(1));
                var decision = d.First();
                Assert.That(decision.DecisionType,Is.EqualTo(DecisionType.CompleteWorkflowExecution));
                return true;
            };
            _amazonWorkflowClient.Verify(w=>w.RespondDecisionTaskCompletedAsync(It.Is<RespondDecisionTaskCompletedRequest>(r=>decisions(r)),
                                                                                It.IsAny<CancellationToken>()),Times.Once);
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