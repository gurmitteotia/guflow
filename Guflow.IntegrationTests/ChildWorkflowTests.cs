// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class ChildWorkflowTests
    {
        private WorkflowHost _workflowHost;
        private ActivityHost _activityHost;
        private TestDomain _domain;
        private static string _taskListName;
        private static string ActivityResult = "some result";
        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _activityHost = await HostAsync(typeof(TestActivity));
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
            _activityHost.StopExecution();
        }

        [Test]
        public async Task Schedule_child_workflow()
        {
            var @event = new ManualResetEvent(false);
            string result = null;
            var workflow = new ParentWorkflow();
            workflow.Completed += (s, e) =>
            {
                result = e.Result;
                @event.Set();
            };
            _workflowHost = await HostAsync(workflow, new ChildWorkflow());

            await _domain.StartWorkflow<ParentWorkflow>("input", _taskListName);
            @event.WaitOne();

            Assert.That(result, Is.EqualTo(ActivityResult));
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
        private class ParentWorkflow : Workflow
        {
            public ParentWorkflow()
            {
                ScheduleChildWorkflow<ChildWorkflow>().OnTaskList(_ => _taskListName);

                ScheduleAction(i => CompleteWorkflow(i.ParentChildWorkflow().Result())).AfterChildWorkflow<ChildWorkflow>();
            }
        }

        [WorkflowDescription(Names.Workflow.Child.Version, Name = Names.Workflow.Child.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class ChildWorkflow : Workflow
        {
            public ChildWorkflow()
            {
                ScheduleActivity<TestActivity>().OnTaskList((t) => _taskListName);
                ScheduleAction(i => CompleteWorkflow(i.ParentActivity<TestActivity>().Result()))
                    .AfterActivity<TestActivity>();
            }
        }


        [ActivityDescription(Names.Activity.Test.Version, Name = Names.Activity.Test.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TestActivity : Activity
        {
            [ActivityMethod]
            public async Task<string> Execute()
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return ActivityResult;
            }
        }
    }
}