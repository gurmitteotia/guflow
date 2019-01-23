// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class WaitForSignalTests
    {
        private WorkflowHost _workflowHost;
        private ActivityHost _activityHost;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            Log.Register(Log.ConsoleLogger);
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _activityHost = await HostAsync(typeof(EmailConfirmActivity), typeof(ActivateUser), typeof(OrderActivity), typeof(ChargeCustomer));
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
            _activityHost.StopExecution();
        }

        [Test]
        public async Task Wait_for_a_signal_to_continue()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new UserActivateWorkflow(@event);
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<UserActivateWorkflow>("input", _taskListName);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "EmailConfirmed", "");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("Activated"));
        }

        [Test]
        public async Task Wait_for_a_signal_to_reschedule()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new OrderWorkflow(@event);
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<OrderWorkflow>("input", _taskListName);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "ItemsArrived", "");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("Charged"));
        }

        private async Task<WorkflowHost> HostAsync(params Workflow[] workflows)
        {
            var hostedWorkflows = await _domain.Host(workflows);
            hostedWorkflows.StartExecution(new TaskList(_taskListName));
            return hostedWorkflows;
        }

        private async Task<ActivityHost> HostAsync(params Type[] activityTypes)
        {
            var hostedActivities = await _domain.Host(activityTypes);
            hostedActivities.StartExecution(new TaskList(_taskListName));
            return hostedActivities;
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class UserActivateWorkflow : Workflow
        {
            private readonly AutoResetEvent _event;

            public UserActivateWorkflow(AutoResetEvent @event)
            {
                _event = @event;
                ScheduleActivity<EmailConfirmActivity>()
                    .OnTaskList(a => _taskListName)
                    .OnCompletion(e =>
                    {
                        _event.Set();
                        return e.WaitForSignal("EmailConfirmed");
                    });

                ScheduleActivity<ActivateUser>()
                    .OnTaskList(a => _taskListName)
                    .AfterActivity<EmailConfirmActivity>();

                ScheduleAction(i => CompleteWorkflow(i.ParentActivity<ActivateUser>().Result()))
                    .AfterActivity<ActivateUser>();
            }
        }

        [ActivityDescription(Names.Activity.EmailConfirm.Version, Name = Names.Activity.EmailConfirm.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class EmailConfirmActivity : Activity
        {
            [ActivityMethod]
            public ActivityResponse OrderItem(string input)
            {
               return Complete("Done");
            }
        }

        [ActivityDescription(Names.Activity.ActivateUser.Version, Name = Names.Activity.ActivateUser.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
         DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class ActivateUser : Activity
        {
            [ActivityMethod]
            public string SendEmail()
            {
                return "Activated";
            }
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
        DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class OrderWorkflow : Workflow
        {
            private readonly AutoResetEvent _event;

            public OrderWorkflow(AutoResetEvent @event)
            {
                _event = @event;
                ScheduleActivity<OrderActivity>()
                    .OnTaskList(a => _taskListName)
                    .OnFailure(e =>
                    {
                        _event.Set();
                        return e.WaitForSignal("ItemsArrived").ToReschedule();
                    });

                ScheduleActivity<ChargeCustomer>()
                    .OnTaskList(a => _taskListName)
                    .AfterActivity<OrderActivity>();

                ScheduleAction(i => CompleteWorkflow(i.ParentActivity().Result()))
                    .AfterActivity<ChargeCustomer>();
            }
        }

        [ActivityDescription(Names.Activity.OrderItem.Version, Name = Names.Activity.OrderItem.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class OrderActivity : Activity
        {
            private static int ExecutionTimes = 0;
            [ActivityMethod]
            public ActivityResponse OrderItem(string input)
            {
                if (ExecutionTimes++ == 0)
                    return Fail("NoItemsLeft", "");

                return Complete("Done");
            }
        }

        [ActivityDescription(Names.Activity.ChargeCustomer.Version, Name = Names.Activity.ChargeCustomer.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
         DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class ChargeCustomer : Activity
        {
            [ActivityMethod]
            public string Execute()
            {
                return "Charged";
            }
        }
    }
}