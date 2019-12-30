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


        [Test]
        public async Task Wait_for_any_signal_with_timeout_and_continue_with_approved_signal()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new ExpenseAnySignalWorkflowWithTimeout(@event,TimeSpan.FromSeconds(2));
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<ExpenseAnySignalWorkflowWithTimeout>("input", _taskListName, _configuration["LambdaRole"]);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "Approved", "");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("\"AccountDone\""));
        }

        [Test]
        public async Task Wait_for_any_signal_with_timeout_and_continue_with_timeout()
        {
            var @event = new AutoResetEvent(false);
            var timeout = TimeSpan.FromSeconds(2);
            var workflow = new ExpenseAnySignalWorkflowWithTimeout(@event, timeout);
            string result = null;
            workflow.Failed += (s, e) => { result = e.Reason; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<ExpenseAnySignalWorkflowWithTimeout>("input", _taskListName, _configuration["LambdaRole"]);
            @event.WaitOne();

            @event.WaitOne(timeout.Add(TimeSpan.FromSeconds(3)));

            Assert.That(result, Is.EqualTo("Signal_timedout"));
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

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
            DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class ExpenseAnySignalWorkflowWithTimeout : Workflow
        {
            public ExpenseAnySignalWorkflowWithTimeout(AutoResetEvent @event, TimeSpan timeout)
            {
                ScheduleLambda("ExpenseApproval").WithInput(_ => new { Id })
                    .OnCompletion(e =>
                    {
                        @event.Set();
                        return e.WaitForAnySignal("Approved", "Rejected").For(timeout);
                    });

                ScheduleLambda("SubmitToAccount").AfterLambda("ExpenseApproval")
                    .When(_ => Signal("Approved").IsTriggered());

                ScheduleAction(i => CompleteWorkflow(i.ParentLambda().Result())).AfterLambda("SubmitToAccount");

                ScheduleLambda("ExpenseRejected").AfterLambda("ExpenseApproval")
                    .When(_ => Signal("Rejected").IsTriggered());
                ScheduleAction(i => CompleteWorkflow(i.ParentLambda().Result())).AfterLambda("ExpenseRejected");

                ScheduleAction(_ => FailWorkflow("Signal_timedout", "")).AfterLambda("ExpenseApproval")
                    .When(_ => AnySignal("Approved", "Rejected").IsTimedout());

            }
        }
    }
}