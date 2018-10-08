// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class ChildWorkflowItemTests
    {
        private EventGraphBuilder _builder;
        private Mock<IWorkflow> _workflow;
        private Identity _identity;
        private const string WorkflowName = "Workflow";
        private const string Version = "1.0";
        private const string PositionalName = "Pos";
        [SetUp]
        public void Setup()
        {
            _identity = Identity.New(WorkflowName, Version, PositionalName);
            _builder = new EventGraphBuilder();
            _workflow = new Mock<IWorkflow>();
            _workflow.SetupGet(w => w.WorkflowHistoryEvents)
                .Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph("input")));
        }

        [Test]
        public void By_default_schedule_with_workflow_input()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);

            var decisions = item.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.StartChildWorkflowExecutionDecisionAttributes.Input , Is.EqualTo("input"));
        }

        [Test]
        public void Input_can_be_customized()
        {
            var workflow = new Mock<IWorkflow>();
            const string workflowInput = "\"input\"";
            workflow.SetupGet(w => w.WorkflowHistoryEvents).Returns(new WorkflowHistoryEvents(_builder.WorkflowStartedGraph(workflowInput)));
            var item = new ChildWorkflowItem(_identity, workflow.Object);
            item.WithInput(_ => new{Id=1});

            var decisions = item.ScheduleDecisions();
            var swfDecision = decisions.Single().SwfDecision();

            Assert.That(swfDecision.StartChildWorkflowExecutionDecisionAttributes.Input, Is.EqualTo("{\"Id\":1}"));
        }

        [Test]
        public void Schedule_the_child_workflow()
        {
            var description = new WorkflowDescription("1.0")
            {
                DefaultChildPolicy = "child",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(3),
                DefaultLambdaRole = "lambdarole",
                DefaultTaskListName = "task",
                DefaultTaskPriority = 1,
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(1),
            };

            var item = new ChildWorkflowItem(_identity, _workflow.Object, description);
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId , Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN , Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("child"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("3"));
            Assert.That(attr.LambdaRole, Is.EqualTo("lambdarole"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("task"));
            Assert.That(attr.TaskPriority, Is.EqualTo("1"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("1"));
            Assert.That(attr.TagList, Is.Empty);
        }

        [Test]
        public void Schedule_the_child_workflow_without_providing_workflow_description()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy, Is.Null);
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.Null);
            Assert.That(attr.LambdaRole, Is.Null);
            Assert.That(attr.TaskList, Is.Null);
            Assert.That(attr.TaskPriority, Is.Null);
            Assert.That(attr.TaskStartToCloseTimeout, Is.Null);
            Assert.That(attr.TagList, Is.Empty);
        }

        [Test]
        public void Can_override_scheduling_properties_when_workflow_description_is_provided()
        {
            var description = new WorkflowDescription("1.0")
            {
                DefaultChildPolicy = "child",
                DefaultExecutionStartToCloseTimeout = TimeSpan.FromSeconds(3),
                DefaultLambdaRole = "lambdarole",
                DefaultTaskListName = "task",
                DefaultTaskPriority = 1,
                DefaultTaskStartToCloseTimeout = TimeSpan.FromSeconds(1),
            };

            var item = new ChildWorkflowItem(_identity, _workflow.Object, description);
            item.WithChildPolicy(_ => "newchild").WithLambdaRole(_ => "newlambda").OnTaskList(_ => "newtask")
                .WithPriority(_ => 2).WithTimeouts(_ => new WorkflowTimeouts()
                {
                    ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(4),
                    TaskStartToCloseTimeout = TimeSpan.FromSeconds(5)
                }).WithTags(_ => new []{"hello", "hi"});
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("newchild"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("4"));
            Assert.That(attr.LambdaRole, Is.EqualTo("newlambda"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("newtask"));
            Assert.That(attr.TaskPriority, Is.EqualTo("2"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("5"));
            Assert.That(attr.TagList, Is.EqualTo(new[]{"hello", "hi"}));
        }

        [Test]
        public void Can_override_scheduling_properties_when_workflow_description_is_not_provided()
        {
            var item = new ChildWorkflowItem(_identity, _workflow.Object);
            item.WithChildPolicy(_ => "newchild").WithLambdaRole(_ => "newlambda").OnTaskList(_ => "newtask")
                .WithPriority(_ => 2).WithTimeouts(_ => new WorkflowTimeouts()
                {
                    ExecutionStartToCloseTimeout = TimeSpan.FromSeconds(4),
                    TaskStartToCloseTimeout = TimeSpan.FromSeconds(5)
                }).WithTags(_ => new[] { "hello", "hi" });
            var swfDecision = item.ScheduleDecisions().First().SwfDecision();

            Assert.That(swfDecision.DecisionType, Is.EqualTo(DecisionType.StartChildWorkflowExecution));
            var attr = swfDecision.StartChildWorkflowExecutionDecisionAttributes;
            Assert.That(attr.WorkflowType.Name, Is.EqualTo(WorkflowName));
            Assert.That(attr.WorkflowType.Version, Is.EqualTo(Version));
            Assert.That(attr.WorkflowId, Is.EqualTo(_identity.Id.ToString()));
            Assert.That(attr.Control.As<ScheduleData>().PN, Is.EqualTo(_identity.PositionalName));
            Assert.That(attr.ChildPolicy.Value, Is.EqualTo("newchild"));
            Assert.That(attr.ExecutionStartToCloseTimeout, Is.EqualTo("4"));
            Assert.That(attr.LambdaRole, Is.EqualTo("newlambda"));
            Assert.That(attr.TaskList.Name, Is.EqualTo("newtask"));
            Assert.That(attr.TaskPriority, Is.EqualTo("2"));
            Assert.That(attr.TaskStartToCloseTimeout, Is.EqualTo("5"));
            Assert.That(attr.TagList, Is.EqualTo(new[] { "hello", "hi" }));
        }

        [Test]
        public void Invalid_arguments()
        {
            var childWorkflowItem = new ChildWorkflowItem(_identity, Mock.Of<IWorkflow>());
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCompletion(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnFailure(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnCancelled(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTerminated(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTimedout(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnStartFailed(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithInput(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithChildPolicy(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithLambdaRole(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.OnTaskList(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithPriority(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithTimeouts(null));
            Assert.Throws<ArgumentNullException>(() => childWorkflowItem.WithTags(null));
        }
    }
}