using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowTaskTests
    {
        private Mock<IAmazonSimpleWorkflow> _amazonWorkflowClient;
        private Domain _domain;
        private HistoryEventsBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            _amazonWorkflowClient = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain("name", _amazonWorkflowClient.Object);
        }

        [Test]
        public async Task On_execution_send_decisions_to_amazon_swf()
        {
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new WorkflowCompleteOnSignal("result") });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

             await workflowTasks.ExecuteForAsync(hostedWorkflows);

             AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token", Times.Once());
        }

        [Test]
        public async Task Throws_exception_when_workflow_history_events_are_queried_after_execution()
        {
            var hostedWorkflow = new WorkflowCompleteOnSignal();
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { hostedWorkflow });
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);
            await workflowTasks.ExecuteForAsync(hostedWorkflows);

            Assert.Throws<InvalidOperationException>(() => hostedWorkflow.AccessHistoryEvents());
        }

        [Test]
        public async Task Does_not_send_response_to_amazon_swf_for_empty_tasks()
        {
            var workflowTasks = WorkflowTask.Empty;

            await workflowTasks.ExecuteForAsync(new WorkflowsHost(_domain, new[] {new WorkflowCompleteOnSignal("result")}));

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
        public async Task Raise_workflow_closed_event_when_workflow_completed_decision_is_delivered_to_amazon_swf()
        {
            WorkflowClosedEventArgs eventArgs = null;
            var workflow = new WorkflowCompleteOnSignal("result");
            workflow.Closed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
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
        public async Task Raise_workflow_closed_event_when_workflow_failed_decision_is_delivered_to_amazon_swf()
        {
            WorkflowClosedEventArgs eventArgs = null;
            var workflow = new WorkflowFailOnSignal("reason", "detail");
            workflow.Closed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
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

        [Test]
        public async Task Raise_workflow_closed_event_when_workflow_cancelled_decision_is_delivered_to_amazon_swf()
        {
            WorkflowClosedEventArgs eventArgs = null;
            var workflow = new WorkflowCancelOnSignal("detail");
            workflow.Closed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
        }

        [Test]
        public async Task Raise_workflow_restarted_event_when_workflow_restarted_decision_is_delivered_to_amazon_swf()
        {
            WorkflowRestartedEventArgs eventArgs = null;
            var workflow = new WorkflowRestartOnSignal();
            workflow.Restarted += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
        }


        [Test]
        public async Task Raise_workflow_closed_event_when_workflow_restarted_decision_is_delivered_to_amazon_swf()
        {
            WorkflowClosedEventArgs eventArgs = null;
            var workflow = new WorkflowRestartOnSignal();
            workflow.Closed += (s, e) => { eventArgs = e; };

            await ExecuteWorkflowOnSignalEvent(workflow, "wid", "runid");

            Assert.That(eventArgs, Is.Not.Null);
            Assert.That(eventArgs.WorkflowId, Is.EqualTo("wid"));
            Assert.That(eventArgs.WorkflowRunId, Is.EqualTo("runid"));
        }

        [Test]
        public async Task  By_default_response_exception_are_unhandled()
        {
            _amazonWorkflowClient.Setup(c=>c.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                                 .Throws(new UnknownResourceException("msg"));

            await ExecuteWorkflowOnSignalEvent(new WorkflowCompleteOnSignal(), "wid", "rid");

            _amazonWorkflowClient.Verify(c=>c.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(),
                It.IsAny<CancellationToken>()),Times.Once);
        }

        [Test]
        public async Task Response_exception_can_be_handled_to_retry()
        {
            _amazonWorkflowClient.SetupSequence(c => c.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                                .Throws(new UnknownResourceException("msg"))
                                .Returns(Task.FromResult(new RespondDecisionTaskCompletedResponse()));
            var hostedWorkflows = new WorkflowsHost(_domain, new[] {new WorkflowCompleteOnSignal()});
            hostedWorkflows.OnResponseError(e => ErrorAction.Retry);
            
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

            await workflowTasks.ExecuteForAsync(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token", Times.Exactly(2));
        }

        [Test]
        public async Task Response_exception_can_be_handled_to_continue()
        {
            _amazonWorkflowClient.SetupSequence(c => c.RespondDecisionTaskCompletedAsync(It.IsAny<RespondDecisionTaskCompletedRequest>(), It.IsAny<CancellationToken>()))
                                .Throws(new UnknownResourceException("msg"))
                                .Returns(Task.FromResult(new RespondDecisionTaskCompletedResponse()));
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new WorkflowCompleteOnSignal() });
            hostedWorkflows.OnResponseError(e => ErrorAction.Continue);
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);

            await workflowTasks.ExecuteForAsync(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token", Times.Once());
        }


        [Test]
        public void By_default_execution_exceptions_are_unhandled()
        {
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new WorkflowThrowsExceptionOnSignal(new ApplicationException("")) });
            
            Assert.ThrowsAsync<ApplicationException>(async ()=>await workflowTasks.ExecuteForAsync(hostedWorkflows));
        }

        [Test]
        public async Task Execution_exception_can_handled_to_retry()
        {
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents("token"), _domain);
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new WorkflowThrowsExceptionOnSignal(new ApplicationException("")) });
            workflowTasks.OnExecutionError(ErrorHandler.Default(e=>ErrorAction.Retry));

            await workflowTasks.ExecuteForAsync(hostedWorkflows);

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token", Times.Once());
        }

        private async Task ExecuteWorkflowOnSignalEvent(Workflow workflow, string workflowId, string runId)
        {
            var hostedWorkflows = new WorkflowsHost(_domain, new[] {workflow});
            var workflowTasks = WorkflowTask.CreateFor(DecisionTasksWithSignalEvents(workflowId, runId), _domain);
            await workflowTasks.ExecuteForAsync(hostedWorkflows);
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient(string token, Times times)
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
                                                                                It.IsAny<CancellationToken>()), times);
        }

        private DecisionTask DecisionTasksWithSignalEvents(string token)
        {
            var historyEvent = _builder.WorkflowSignaledEvent("name", "input");
            return new DecisionTask
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow", Version = "1.0" },
                Events = new List<HistoryEvent>(){ historyEvent},
                PreviousStartedEventId = 0,
                StartedEventId = historyEvent.EventId,
                TaskToken = token,
                WorkflowExecution = new WorkflowExecution() { RunId = "rid", WorkflowId = "wid"}
            }; 
        }

        private DecisionTask DecisionTasksWithSignalEvents(string workflowId, string runId)
        {
            var workflowStartedEvent = _builder.WorkflowStartedEvent("input");
            var historyEvent = _builder.WorkflowSignaledEvent("name", "input");
            return new DecisionTask
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow", Version = "1.0" },
                Events = new List<HistoryEvent>() { historyEvent, workflowStartedEvent },
                PreviousStartedEventId = 0,
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
                var active = HasActiveEvent;
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

        [WorkflowDescription("1.0", Name = "TestWorkflow")]
        private class WorkflowRestartOnSignal : Workflow
        {
            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return RestartWorkflow();
            }
           
        }
        [WorkflowDescription("1.0", Name = "TestWorkflow")]
        private class WorkflowThrowsExceptionOnSignal : Workflow
        {
            private readonly Exception _exception;
            private int _signalCounts = 0;
            public WorkflowThrowsExceptionOnSignal(Exception exception)
            {
                _exception = exception;
            }
            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                _signalCounts++;
                if (_signalCounts == 2)
                    return CompleteWorkflow("something");
                throw _exception;
            }
        }

    }
}