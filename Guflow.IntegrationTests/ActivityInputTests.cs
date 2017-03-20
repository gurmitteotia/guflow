using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class ActivityInputTests
    {
        private HostedWorkflows _hostedWorkflows;
        private HostedActivities _hostedActivities;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _hostedActivities = await HostAsync(typeof(TestActivityWithInput));
        }

        [TearDown]
        public void TearDown()
        {
            _hostedWorkflows.StopExecution();
            _hostedActivities.StopExecution();
        }

        [Test]
        public async Task By_default_schedule_the_activity_with_workflow_input()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowToScheduleActivityWithDefaultInput();
            workflow.Closed += (s, e) => @event.Set();
            _hostedWorkflows = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowToScheduleActivityWithDefaultInput>("input", _taskListName);
            @event.WaitOne();

            Assert.That(TestActivityWithInput.Input, Is.EqualTo("input"));
        }

        [Test]
        public async Task Can_schedule_the_activity_with_custom_input()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowToScheduleActivityWithCustomInput("activity input");
            workflow.Closed += (s, e) => @event.Set();
            _hostedWorkflows = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowToScheduleActivityWithCustomInput>("input", _taskListName);
            @event.WaitOne();

            Assert.That(TestActivityWithInput.Input, Is.EqualTo("activity input"));
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

        [WorkflowDescription("1.0", Name  = "TestWorkflow", DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToScheduleActivityWithDefaultInput : Workflow
        {
            public WorkflowToScheduleActivityWithDefaultInput()
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t)=>_taskListName);
            }
        }

        [WorkflowDescription("1.0", Name = "TestWorkflow", DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
          DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToScheduleActivityWithCustomInput : Workflow
        {
            public WorkflowToScheduleActivityWithCustomInput(string activityInput)
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t) => _taskListName).WithInput(t => activityInput);
            }
        }


        [ActivityDescription("1.0", Name = "TestActivity", DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10,  Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TestActivityWithInput : Activity
        {
            [Execute]
            public string Execute(string input)
            {
                Input = input;
                return input;
            }

            public static string Input;
        }
    }
}