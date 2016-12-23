using System;
using System.Linq;
using Amazon.SimpleWorkflow;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
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
        public void Throws_exception_multiple_hosted_workflows_execution_start_without_providing_the_task_queue()
        {
            var hostedWorkflows = _domain.Host(new Workflow[] {new TestWorkflow1(), new TestWorkflow2()});

            Assert.Throws<InvalidOperationException>(() => hostedWorkflows.StartExecution());
        }

        [Test]
        public void Throws_exception_when_default_task_list_is_not_provided_and_execution_start_without_providing_the_task_queue()
        {
            var hostedWorkflows = _domain.Host(new Workflow[] { new TestWorkflow1() });

            Assert.Throws<InvalidOperationException>(() => hostedWorkflows.StartExecution());
        }

        [Test]
        public void Throws_exception_when_execution_start_with_null_task_queue()
        {
            var hostedWorkflows = _domain.Host(new Workflow[] { new TestWorkflow1() });

            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.StartExecution((TaskQueue)null));
        }

        [Test]
        public void Invalid_constructor_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(() => new HostedWorkflows(null, new[] {new TestWorkflow1()}));
            Assert.Throws<ArgumentNullException>(() => new HostedWorkflows(_domain, null));
            Assert.Throws<ArgumentException>(() => new HostedWorkflows(_domain, Enumerable.Empty<Workflow>()));
            Assert.Throws<ArgumentException>(() => new HostedWorkflows(_domain, new []{(Workflow)null}));
        }

        [Test]
        public void Invalid_error_handler_argument_tests()
        {
            var hostedWorkflows = new HostedWorkflows(_domain, new []{new TestWorkflow1()});

            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnError((HandleError) null));
            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnError((IErrorHandler)null));
            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnResponseError((HandleError)null));
            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnResponseError((IErrorHandler)null));
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