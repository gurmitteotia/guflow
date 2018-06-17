// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class LamdbaItemTests
    {
        private HistoryEventsBuilder _builder;
        private Mock<IWorkflow> _workflow;
        [SetUp]
        public void Setup()
        {
            _builder = new HistoryEventsBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph()));
        }
        [Test]
        public void Schedule_lamdba_function()
        {
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), _workflow.Object);

            var decisions = lambdaItem.GetScheduleDecisions();

            Assert.That(decisions, Is.EqualTo(new []{new ScheduleLambdaDecision(Identity.Lambda("name"), "does not matter")}));
        }

        [Test]
        public void By_default_lambda_function_is_scheduled_with_workflow_input()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), workflow.Object);

            var decisions = lambdaItem.GetScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo(workflowInput));
        }

        [Test]
        public void Input_of_lambda_function_can_be_customized()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "actvity";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), workflow.Object);
            lambdaItem.WithInput(i => "CustomInput");
            var decisions = lambdaItem.GetScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.Input, Is.EqualTo("CustomInput"));
        }

        [Test]
        public void Invalid_argument_tests()
        {
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), Mock.Of<IWorkflow>());
            Assert.Throws<ArgumentNullException>(() => lambdaItem.WithInput(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.WithTimeout(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnTimedout(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnSchedulingFailed(null));
            Assert.Throws<ArgumentNullException>(() => lambdaItem.OnStartFailed(null));
        }

        [Test]
        public void Cancel_decision_is_empty()
        {
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), Mock.Of<IWorkflow>());
            Assert.That(lambdaItem.GetCancelDecision(), Is.EqualTo(WorkflowDecision.Empty));
        }

        [Test]
        public void By_default_timeout_of_lamdba_fuction_is_null()
        {
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), _workflow.Object);

            var swfDecision = lambdaItem.GetScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.Null);
        }

        [Test]
        public void Time_out_scheduling_lambda_function_can_be_customized()
        {
            var lambdaItem = new LambdaItem(Identity.Lambda("name"), _workflow.Object);
            lambdaItem.WithTimeout(i => TimeSpan.FromSeconds(10));
            var swfDecision = lambdaItem.GetScheduleDecisions().Single().SwfDecision();

            Assert.That(swfDecision.ScheduleLambdaFunctionDecisionAttributes.StartToCloseTimeout, Is.EqualTo("10"));
        }

        [Test]
        public void Reschedule_decision_is_a_timer_decision_for_lambda_item()
        {
            var identity = Identity.Lambda("name");
            var lambdaItem = new LambdaItem(identity, _workflow.Object);
            var decision = lambdaItem.GetRescheduleDecisions(TimeSpan.FromSeconds(10));
            Assert.That(decision, Is.EqualTo(new []{new ScheduleTimerDecision(identity, TimeSpan.FromSeconds(10), true)}));
        }
    }
}