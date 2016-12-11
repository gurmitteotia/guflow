using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private Domain _domain;
        [SetUp]
        public void Setup()
        {
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain("name",new Mock<IAmazonSimpleWorkflow>().Object);
        }

        [Test]
        public async Task Interpret_new_events_for_hosted_workflow_are_sent_to_amazon_swf()
        {
            var hostedWorkflows = new HostedWorkflows(_domain, new[] { new TestWorkflow("result") });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

             await workflowTasks.ExecuteFor(hostedWorkflows);

             AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token");
        }

        [Test]
        public async Task Workflow_history_events_can_not_be_queried_after_execution()
        {
            var hostedWorkflow = new TestWorkflow();
            var hostedWorkflows = new HostedWorkflows(_domain, new[] { hostedWorkflow });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);
            await workflowTasks.ExecuteFor(hostedWorkflows);

            Assert.Throws<InvalidOperationException>(() => hostedWorkflow.AccessHistoryEvents());
        }

       
        [Test]
        public async Task Does_not_send_response_to_amazon_swf_for_empty_tasks()
        {
            var workflowTasks = WorkflowTask.Empty;

            await workflowTasks.ExecuteFor(new HostedWorkflows(_domain, new[] {new TestWorkflow("result")}));

            _amazonWorkflowClient.Verify(w => w.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(),
                                                                               It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Raise_workflow_completed_event_when_workflow_completed_decision_is_delivered_to_amazon_swf()
        {
            var workflow = new TestWorkflow("result");
            WorkflowCompletedEventArgs eventArgs = null;
            workflow.Completed += (s, e) => { eventArgs = e; };
            var hostedWorkflows = new HostedWorkflows(_domain, new[] { workflow });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

            await workflowTasks.ExecuteFor(hostedWorkflows);

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
            Assert.That(eventArgs.Result, Is.EqualTo("result"));
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

        private static DecisionTask DecisionTasksWithSignalEvents(string token)
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
            private readonly string _completeResult;

            public TestWorkflow(string completeResult = null)
            {
                _completeResult = completeResult;
            }

            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return CompleteWorkflow(_completeResult);
            }

            public void AccessHistoryEvents()
            {
                var active = IsActive;
            }
        }

      
    }
}