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
        private WorkflowHost _workflowHost;
        private ActivityHost _activityHost;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _activityHost = await HostAsync(typeof(TestActivityWithInput));
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
            _activityHost.StopExecution();
        }

        [Test]
        public async Task By_default_schedule_the_activity_with_workflow_input()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowToScheduleActivityWithDefaultInput();
            workflow.Closed += (s, e) => @event.Set();
            _workflowHost = await HostAsync(workflow);

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
            _workflowHost = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowToScheduleActivityWithCustomInput>("input", _taskListName);
            @event.WaitOne();

            Assert.That(TestActivityWithInput.Input, Is.EqualTo("activity input"));
        }

        [Test]
        public async Task Can_schedule_the_activity_with_custom_input_built_from_workflow_input()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowToAccessDynamicInput();
            workflow.Closed += (s, e) => @event.Set();
            _workflowHost = await HostAsync(workflow);
            var input = new {Name = "name", Age = 10}.ToJson();
            
            await _domain.StartWorkflow<WorkflowToScheduleActivityWithCustomInput>(input, _taskListName);
            @event.WaitOne();

            Assert.That(TestActivityWithInput.Input, Is.EqualTo(input));
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

        [WorkflowDescription(Names.Workflow.Test.Version, Name  = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToScheduleActivityWithDefaultInput : Workflow
        {
            public WorkflowToScheduleActivityWithDefaultInput()
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t)=>_taskListName);
            }
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
          DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToScheduleActivityWithCustomInput : Workflow
        {
            public WorkflowToScheduleActivityWithCustomInput(string activityInput)
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t) => _taskListName).WithInput(t => activityInput);
            }
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToAccessDynamicInput : Workflow
        {
            public WorkflowToAccessDynamicInput()
            {
                ScheduleActivity<TestActivityWithInput>().OnTaskList((t) => _taskListName)
                    .WithInput(t => new {Input.Name, Input.Age});
            }
        }


        [ActivityDescription(Names.Activity.Test.Version, Name = Names.Activity.Test.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10,  Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TestActivityWithInput : Activity
        {
            [ActivityMethod]
            public string Execute(string input)
            {
                Input = input;
                return input;
            }

            public static string Input;
        }
    }
}