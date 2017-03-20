using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class WorkflowWithOneActivityTests
    {
        private HostedWorkflows _hostedWorkflows;
        private HostedActivities _hostedActivities;
        private Workflow _workflow;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _workflow = new TestWorkflow();
            _hostedWorkflows = await _domain.Host(_workflow);
            _hostedActivities = await _domain.Host(new []{typeof(TestActivityWithResult)});
            _taskListName = Guid.NewGuid().ToString();
            _hostedWorkflows.StartExecution(new TaskQueue(_taskListName));
            _hostedActivities.StartExecution(new TaskQueue(_taskListName));
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
            string completeResult = null;
            _workflow.Completed += (s, e) => {completeResult= e.Result;};
            _workflow.Closed += (s, e) => @event.Set();
            await _domain.StartWorkflow<TestWorkflow>("input", _taskListName);
            @event.WaitOne();

            Assert.That(completeResult, Is.EqualTo("input"));
        }

        [WorkflowDescription("1.0", Name  = "TestWorkflow", DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class TestWorkflow : Workflow
        {
            public TestWorkflow()
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t)=>_taskListName);
            }
        }


        [ActivityDescription("1.0", Name = "TestActivity", DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10,  Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TestActivityWithResult : Activity
        {
            [Execute]
            public string Execute(string input)
            {
                return input;
            }
        }
    }
}