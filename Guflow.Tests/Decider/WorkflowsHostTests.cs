using System;
using System.Linq;
using System.Threading;
using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;
using Guflow.Decider;
using Moq;
using NUnit.Framework;

namespace Guflow.Tests.Decider
{
    [TestFixture]
    public class WorkflowsHostTests
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
            var hostedWorkflows = new WorkflowsHost(_domain, new Workflow[] { hostedWorkflow1, hostedWorkflow2 });

            Assert.That(hostedWorkflows.FindBy("TestWorkflow1", "2.0"), Is.EqualTo(hostedWorkflow1));
            Assert.That(hostedWorkflows.FindBy("TestWorkflow2", "1.0"), Is.EqualTo(hostedWorkflow2));
        }

        [Test]
        public void Throws_exception_when_hosted_workflow_is_not_found()
        {
            var hostedWorkflow = new TestWorkflow1();
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { hostedWorkflow });

            Assert.Throws<WorkflowNotHostedException>(() => hostedWorkflows.FindBy("TestWorkflow2", "2.0"));
        }

        [Test]
        public void Throws_exception_when_same_workflow_is_hosted_twice()
        {
            var hostedWorkflow1 = new TestWorkflow1();
            var hostedWorkflow2 = new TestWorkflow1();
            Assert.Throws<WorkflowAlreadyHostedException>(() => new WorkflowsHost(_domain, new Workflow[] { hostedWorkflow1, hostedWorkflow2 }));
        }

        [Test]
        public void Throws_exception_multiple_hosted_workflows_execution_start_without_providing_the_task_queue()
        {
            var hostedWorkflows = _domain.Host(new Workflow[] { new TestWorkflow1(), new TestWorkflow2() });

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

            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.StartExecution((TaskList)null));
        }

        [Test]
        public void Invalid_constructor_argument_tests()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkflowsHost(null, new[] { new TestWorkflow1() }));
            Assert.Throws<ArgumentNullException>(() => new WorkflowsHost(_domain, null));
            Assert.Throws<ArgumentException>(() => new WorkflowsHost(_domain, Enumerable.Empty<Workflow>()));
            Assert.Throws<ArgumentException>(() => new WorkflowsHost(_domain, new[] { (Workflow)null }));
        }

        [Test]
        public void Invalid_error_handler_argument_tests()
        {
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() });

            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnError(null));
            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnResponseError(null));
            Assert.Throws<ArgumentNullException>(() => hostedWorkflows.OnPollingError(null));
        }

        [Test]
        public void Status_is_set_to_initialized_for_new_workflow_host()
        {
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() });
            Assert.That(hostedWorkflows.Status, Is.EqualTo(HostStatus.Initialized));
        }

        [Test]
        public void Status_is_set_to_executing_when_workflow_host_is_executing()
        {
            using (var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() }))
            {
                hostedWorkflows.StartExecution(new TaskList("name"));
                Assert.That(hostedWorkflows.Status, Is.EqualTo(HostStatus.Executing));
            }
        }

        [Test]
        public void Status_is_set_to_stopped_when_workflow_host_is_stopped_execution()
        {
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() });
            hostedWorkflows.StartExecution(new TaskList("name"));
            hostedWorkflows.StopExecution();
            Assert.That(hostedWorkflows.Status, Is.EqualTo(HostStatus.Stopped));
        }

        [Test]
        public void Status_is_set_to_faulted_when_workflow_host_can_not_handle_exception()
        {
            _simpleWorkflow.Setup(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(),
                It.IsAny<CancellationToken>())).Throws<Exception>();

            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() });
            hostedWorkflows.StartExecution(new TaskList("name"));
            hostedWorkflows.StopExecution();
            Assert.That(hostedWorkflows.Status, Is.EqualTo(HostStatus.Faulted));
        }
        [Test]
        public void Raise_faulted_event_on_unhandled_exception()
        {
            var expectedException = new Exception();
            _simpleWorkflow.Setup(s => s.PollForDecisionTaskAsync(It.IsAny<PollForDecisionTaskRequest>(),
                It.IsAny<CancellationToken>())).Throws(expectedException);
            Exception actualException = null;
            var hostedWorkflows = new WorkflowsHost(_domain, new[] { new TestWorkflow1() });
            hostedWorkflows.OnFault += (s, e) => actualException = e.Exception;
            hostedWorkflows.StartExecution(new TaskList("name"));
            hostedWorkflows.StopExecution();
            Assert.That(actualException, Is.EqualTo(expectedException));
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