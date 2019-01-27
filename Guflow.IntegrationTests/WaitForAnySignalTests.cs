// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    public class WaitForAnySignalTests
    {
        private WorkflowHost _workflowHost;
        private TestDomain _domain;
        private static string _taskListName;
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            Log.Register(Log.ConsoleLogger);
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _configuration = Configuration.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
        }

        [Test]
        public async Task Wait_for_approved_signal_to_continue()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new ExpenseAnySignalWorkflow(@event);
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<ExpenseAnySignalWorkflow>("input", _taskListName, _configuration["LambdaRole"]);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "Approved", "");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("\"AccountDone\""));
        }

        [Test]
        public async Task Wait_for_rejected_signal_to_continue()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new ExpenseAnySignalWorkflow(@event);
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<ExpenseAnySignalWorkflow>("input", _taskListName, _configuration["LambdaRole"]);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "Rejected", "");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("\"EmpAction\""));
        }


        private async Task<WorkflowHost> HostAsync(params Workflow[] workflows)
        {
            var hostedWorkflows = await _domain.Host(workflows);
            hostedWorkflows.StartExecution(new TaskList(_taskListName));
            return hostedWorkflows;
        }

      

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class ExpenseAnySignalWorkflow : Workflow
        {
            public ExpenseAnySignalWorkflow(AutoResetEvent @event)
            {
                ScheduleLambda("ExpenseApproval").WithInput(_=>new {Id})
                    .OnCompletion(e =>
                    {
                        @event.Set();
                        return e.WaitForAnySignal("Approved", "Rejected");
                    });

                ScheduleLambda("SubmitToAccount").AfterLambda("ExpenseApproval")
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleAction(i => CompleteWorkflow(i.ParentLambda().Result())).AfterLambda("SubmitToAccount");

                ScheduleLambda("ExpenseRejected").AfterLambda("ExpenseApproval")
                    .When(_ => Signal("Rejected").IsTriggered());
                ScheduleAction(i => CompleteWorkflow(i.ParentLambda().Result())).AfterLambda("ExpenseRejected");

            }
        }
    }
}