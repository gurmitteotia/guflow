using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class SignalWorkflowTests
    {
        private HostedWorkflows _hostedWorkflows;
        private HostedActivities _hostedActivities;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            Log.Register(Log.ConsoleLogger);
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _hostedActivities = await HostAsync(typeof(OrderItemActivity), typeof(ChargeCustomerActivity));
        }

        [TearDown]
        public void TearDown()
        {
            _hostedWorkflows.StopExecution();
            _hostedActivities.StopExecution();
        }

        [Test]
        public async Task On_signal_can_schedule_paused_workflow()
        {
            var @event = new AutoResetEvent(false);
            var workflow = new WorkflowWithMultipleParent(@event);
            string result = null;
            workflow.Completed += (s, e) => { result = e.Result; @event.Set(); };
            _hostedWorkflows = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<WorkflowWithMultipleParent>("input", _taskListName);
            @event.WaitOne();

            await _domain.SendSignal(workflowId, "InventoryFilled", "Enough");
            @event.WaitOne();

            Assert.That(result, Is.EqualTo("Item is on the way"));
        }

        private async Task<HostedWorkflows> HostAsync(params Workflow[] workflows)
        {
            var hostedWorkflows = await _domain.Host(workflows);
            hostedWorkflows.StartExecution(new TaskQueue(_taskListName));
            return hostedWorkflows;
        }

        private async Task<HostedActivities> HostAsync(params Type[] activityTypes)
        {
            var hostedActivities = await _domain.Host(activityTypes);
            hostedActivities.StartExecution(new TaskQueue(_taskListName));
            return hostedActivities;
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowWithMultipleParent : Workflow
        {
            private readonly AutoResetEvent _event;

            public WorkflowWithMultipleParent(AutoResetEvent @event)
            {
                _event = @event;
                ScheduleActivity<OrderItemActivity>()
                    .OnTaskList(a => _taskListName)
                    .OnFailure(PauseOnOutOfStock);

                ScheduleActivity<ChargeCustomerActivity>()
                    .OnTaskList(a => _taskListName)
                    .AfterActivity<OrderItemActivity>();

                ScheduleAction(i => CompleteWorkflow(i.ParentActivity<ChargeCustomerActivity>().Result()))
                    .AfterActivity<ChargeCustomerActivity>();

            }

            [WorkflowEvent(EventName.Signal)]
            public WorkflowAction InventoryUpdated(WorkflowSignaledEvent @event)
            {
                if (!Activities.First<OrderItemActivity>().IsActive)
                    return Jump().ToActivity<OrderItemActivity>();

                return Ignore(false);
            }

            private WorkflowAction PauseOnOutOfStock(ActivityFailedEvent @event)
            {
                if (@event.Reason.Equals("OutOfStock"))
                {
                    _event.Set();
                    return Ignore(true);
                }

                return DefaultAction(@event);
            }

        }

        [ActivityDescription(Names.Activity.OrderItem.Version, Name = Names.Activity.OrderItem.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class OrderItemActivity : Activity
        {
            private static int _executionTimes = 0;
            [Execute]
            public ActivityResponse OrderItem(string input)
            {
                if (++_executionTimes == 1)
                    return Fail("OutOfStock", "No more items available");

                return Complete("Shipped");
            }
        }

        [ActivityDescription(Names.Activity.ChargeCustomer.Version, Name = Names.Activity.ChargeCustomer.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
         DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class ChargeCustomerActivity : Activity
        {
            [Execute]
            public string SendEmail()
            {
                return "Item is on the way";
            }
        }
    }
}