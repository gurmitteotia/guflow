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
    public class HostedWorkflowsTests
    {
        private Domain _domain;
        private Mock<IAmazonSimpleWorkflow> _simpleWorkflow;

        [SetUp]
        public void Setup()
        {
            _simpleWorkflow = new Mock<IAmazonSimpleWorkflow>();
            _domain = new Domain("domain", _simpleWorkflow.Object);
        }
        [Test]
        public void Returns_matching_hosted_workflow_by_name_and_version()
        {
            var hostedWorkflow1 = new TestWorkflow1();
            var hostedWorkflow2 = new TestWorkflow2();
            var hostedWorkflows = new HostedWorkflows(_domain, new Workflow[]{hostedWorkflow1,hostedWorkflow2});

            Assert.That(hostedWorkflows.FindBy("TestWorkflow1","2.0"),Is.EqualTo(hostedWorkflow1));
            Assert.That(hostedWorkflows.FindBy("TestWorkflow2", "1.0"), Is.EqualTo(hostedWorkflow2));
        }

        [Test]
        public void Throws_exception_when_hosted_workflow_is_not_found()
        {
            var hostedWorkflow = new TestWorkflow1();
            var hostedWorkflows = new HostedWorkflows(_domain, new [] { hostedWorkflow });

            Assert.Throws<WorkflowNotHostedException>(()=>hostedWorkflows.FindBy("TestWorkflow2", "2.0"));
        }

        [Test]
        public void Throws_exception_when_same_workflow_is_hosted_twice()
        {
            var hostedWorkflow1 = new TestWorkflow1();
            var hostedWorkflow2 = new TestWorkflow1();
            Assert.Throws<WorkflowAlreadyHostedException>(()=> new HostedWorkflows(_domain, new Workflow[] { hostedWorkflow1, hostedWorkflow2 }));
        }

        [Test]
        public async Task Execute_the_workflow_for_new_tasks_in_loop()
        {
            var hostedWorkflows = _domain.Host(new[] {new TestWorkflow1()});
            var signalTasks1 = CreateDecisionTaskWithSignalEvents("token1");
            var signalTasks2 = CreateDecisionTaskWithSignalEvents("token2");
            SetupSimpleWorkflowToReturn(signalTasks1, signalTasks2);
            int runAttempts = 0;
            hostedWorkflows.Cancelled = ()=>++runAttempts > 2;
            await hostedWorkflows.StartExecutionAsync(new TaskQueue("name"));

            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token1");
            AssertThatInterpretedDecisionsAreSentOverWorkflowClient("token2");
        }

        private void AssertThatInterpretedDecisionsAreSentOverWorkflowClient(string token)
        {
            Func<RespondDecisionTaskCompletedRequest, bool> decisions = (r) =>
            {
                Assert.That(r.TaskToken, Is.EqualTo(token));
                var d = r.Decisions;
                Assert.That(d.Count(), Is.EqualTo(1));
                var decision = d.First();
                Assert.That(decision.DecisionType, Is.EqualTo(DecisionType.CancelWorkflowExecution));
                return true;
            };
            _simpleWorkflow.Verify(w => w.RespondDecisionTaskCompletedAsync(It.Is<RespondDecisionTaskCompletedRequest>(r => decisions(r)),
                                                                                It.IsAny<CancellationToken>()), Times.Once);
        }

        private void SetupSimpleWorkflowToReturn(DecisionTask decisionTask1, DecisionTask decisionTask2)
        {
            _simpleWorkflow.SetupSequence(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse {DecisionTask = decisionTask1}))
                .Returns(Task.FromResult(new PollForDecisionTaskResponse {DecisionTask = decisionTask2}));

        }


        private static DecisionTask CreateDecisionTaskWithSignalEvents(string token)
        {
            var historyEvent = HistoryEventFactory.CreateWorkflowSignaledEvent("name", "input");
            return new DecisionTask()
            {
                WorkflowType = new WorkflowType() { Name = "TestWorkflow1", Version = "2.0" },
                Events = new List<HistoryEvent>() { historyEvent },
                PreviousStartedEventId = historyEvent.EventId,
                StartedEventId = historyEvent.EventId,
                TaskToken = token
            };
        }

        [WorkflowDescription("2.0")]
        private class TestWorkflow1 : Workflow
        {
            [WorkflowEvent(EventName.Signal)]
            private WorkflowAction OnSignal()
            {
                return CancelWorkflow("detail");
            }
        }
        [WorkflowDescription("1.0")]
        private class TestWorkflow2 : Workflow
        {
        }
    }
}