// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class LambdaWorkflowTests
    {
        private WorkflowHost _workflowHost;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            Log.Register(Log.ConsoleLogger);
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
        }


        [Test]
        public async Task Schedule_a_lambda_function()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new ScheduleLambdaWorkflow();
            string result = "result";
            workflow.Closed += (s, e) => @event.Set();
            workflow.Completed += (s, e) => result = e.Result;
            _workflowHost = await HostAsync(workflow);

            await _domain.StartWorkflow<ScheduleLambdaWorkflow>("hotel", _taskListName);
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("resultbooked"));
        }

        private async Task<WorkflowHost> HostAsync(params Workflow[] workflows)
        {
            var hostedWorkflows = await _domain.Host(workflows);
            hostedWorkflows.StartExecution(new TaskList(_taskListName));
            return hostedWorkflows;
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
            DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class ScheduleLambdaWorkflow : Workflow
        {
            public ScheduleLambdaWorkflow()
            {
                ScheduleLambda("BookHotelLambda");

                ScheduleAction(i => CompleteWorkflow(i.ParentLambda().Result()));
            }
        }
    }
}