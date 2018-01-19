// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class EmptyWorkflowTests
    {
        private WorkflowHost _workflowHost;
        private Workflow _emptyWorkflow;
        private TestDomain _domain;
        private string _taskListName;
        
        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _emptyWorkflow = new EmptyWorkflow();
            _workflowHost = await _domain.Host(_emptyWorkflow);
            _taskListName = Guid.NewGuid().ToString();
            _workflowHost.StartExecution(new TaskList(_taskListName));
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
        }

        [Test]
        public async Task Empty_workflow_is_completed_immediatly_when_started()
        {
            var @event = new ManualResetEvent(false);
            _emptyWorkflow.Completed += (s, e) => @event.Set();

            await _domain.StartWorkflow<EmptyWorkflow>("input", _taskListName);
            @event.WaitOne();
        }

        [WorkflowDescription("1.0", DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class EmptyWorkflow : Workflow
        {
            
        }
    }
}
