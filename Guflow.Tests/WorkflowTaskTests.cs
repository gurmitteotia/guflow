﻿using System;
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
            _domain = new Domain("name", _amazonWorkflowClient.Object);
        }

        [Test]
        public async Task Send_interpreted_events_to_amazon_swf()
        {
            var hostedWorkflows = new HostedWorkflows(_domain, new[] { new WorkflowCompleteOnSignal("result") });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

             await workflowTasks.ExecuteForAsync(hostedWorkflows);

             AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token");
        }

        [Test]
        public async Task Workflow_history_events_can_not_be_queried_after_execution()
        {
            var hostedWorkflow = new WorkflowCompleteOnSignal();
            var hostedWorkflows = new HostedWorkflows(_domain, new[] { hostedWorkflow });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);
            await workflowTasks.ExecuteForAsync(hostedWorkflows);

            Assert.Throws<InvalidOperationException>(() => hostedWorkflow.AccessHistoryEvents());
        }

       
        [Test]
        public async Task Does_not_send_response_to_amazon_swf_for_empty_tasks()
        {
            var workflowTasks = WorkflowTask.Empty;

            await workflowTasks.ExecuteForAsync(new HostedWorkflows(_domain, new[] {new WorkflowCompleteOnSignal("result")}));

            _amazonWorkflowClient.Verify(w => w.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(),
                                                                               It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Raise_workflow_completed_event_when_workflow_completed_decision_is_delivered_to_amazon_swf()
        {
            WorkflowCompletedEventArgs eventArgs = null;
            var workflow = new WorkflowCompleteOnSignal("result");
            workflow.Completed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
            Assert.That(eventArgs.Result, Is.EqualTo("result"));
        }

        [Test]
        public async Task Raise_workflow_failed_event_when_workflow_failed_decision_is_delivered_to_amazon_swf()
        {
            WorkflowFailedEventArgs eventArgs = null;
            var workflow = new WorkflowFailOnSignal("reason","detail");
            workflow.Failed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
            Assert.That(eventArgs.Reason, Is.EqualTo("reason"));
            Assert.That(eventArgs.Details, Is.EqualTo("detail"));
        }


        [Test]
        public async Task Raise_workflow_cancelled_event_when_workflow_cancelled_decision_is_delivered_to_amazon_swf()
        {
            WorkflowCancelledEventArgs eventArgs = null;
            var workflow = new WorkflowCancelOnSignal("detail");
            workflow.Cancelled += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
            Assert.That(eventArgs.Details, Is.EqualTo("detail"));
        }

        private async Task ExecuteWorkflowOnSignalEvent(Workflow workflow, string workflowId, string runId)
        {
            var hostedWorkflows = new HostedWorkflows(_domain, new[] {workflow});
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents(workflowId, runId), _domain);
            await workflowTasks.ExecuteForAsync(hostedWorkflows);
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient(string token)
        {
            Func<RespondDecisionTaskCompletedRequest, bool> decisions = (r) =>
            {
                Assert.That(r.TaskToken,Is.EqualTo(token));
                var d = r.Decisions;
                Assert.That(d.Count, Is.EqualTo(1));
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
            return new DecisionTask
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow", Version = "1.0" },
                Events = new List<HistoryEvent>(){ historyEvent},
                PreviousStartedEventId = historyEvent.EventId,
                StartedEventId = historyEvent.EventId,
                TaskToken = token
            }; 
        }

        private static DecisionTask DecisionTasksWithSignalEvents(string workflowId, string runId)
        {
            var historyEvent = HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input");
            return new DecisionTask
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow", Version = "1.0" },
                Events = new List<HistoryEvent>() { historyEvent },
                PreviousStartedEventId = historyEvent.EventId,
                StartedEventId = historyEvent.EventId,
                TaskToken = "token",
                WorkflowExecution = new WorkflowExecution() {  WorkflowId = workflowId, RunId = runId}
            };
        }

        [WorkflowDescription("1.0", Name = "TestWorkflow")]
        private class WorkflowCompleteOnSignal:Workflow
        {
            private readonly string _completeResult;

            public WorkflowCompleteOnSignal(string completeResult = null)
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

        [WorkflowDescription("1.0", Name = "TestWorkflow")]
        private class WorkflowFailOnSignal : Workflow
        {
            private readonly string _reason;
            private readonly string _details;

            public WorkflowFailOnSignal(string reason, string details)
            {
                _reason = reason;
                _details = details;
            }

            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return FailWorkflow(_reason, _details);
            }
        }
        [WorkflowDescription("1.0", Name = "TestWorkflow")]
        private class WorkflowCancelOnSignal : Workflow
        {
            private readonly string _details;

            public WorkflowCancelOnSignal(string details)
            {
                _details = details;
            }

            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return CancelWorkflow(_details);
            }
        }
    }
}