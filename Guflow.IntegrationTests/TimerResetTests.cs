// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class TimerResetTests
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
        public async Task Reset_timer()
        {
            string result = "";
            var @event = new ManualResetEvent(false);
            var workflow = new ResetTimerWorkflow();
            workflow.Closed += (s, e) => @event.Set();
            workflow.Completed += (s, e) => { result = e.Result; };
            _workflowHost = await HostAsync(workflow);

            var workflowId = await _domain.StartWorkflow<ResetTimerWorkflow>("input", _taskListName);
            Assert.True(workflow.WaitForWorkflowStart());
            Thread.Sleep(6000); // TODO: get rid of this in future.
            await _domain.SendSignal(workflowId, "ResetTimer", "");
            @event.WaitOne();

            Assert.That(workflow.TimerIsReset, Is.True);
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
        private class ResetTimerWorkflow : Workflow
        {
            private readonly AutoResetEvent _workflowStarted = new AutoResetEvent(false);
            public ResetTimerWorkflow()
            {
                ScheduleTimer("Timer1").FireAfter(TimeSpan.FromSeconds(10))
                    .OnFired(e =>
                    {
                        if (Timer(e).AllEvents().OfType<TimerCancelledEvent>().Count() == 1)
                            TimerIsReset = true;
                        return Continue(e);
                    });
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t) => _taskListName).AfterTimer("Timer1");

                ScheduleAction(i => CompleteWorkflow(i.ParentActivity().Result())).AfterActivity("TestActivity", "1.0");
            }

            [SignalEvent]
            public WorkflowAction ResetTimer() => Timer("Timer1").IsActive ? Timer("Timer1").Reset() : Ignore;

            [WorkflowEvent(EventName.WorkflowStarted)]
            public WorkflowAction WorkflowStarted()
            {
                _workflowStarted.Set();
                return StartWorkflow();
            }
            public bool TimerIsReset;

            public bool WaitForWorkflowStart() => _workflowStarted.WaitOne(TimeSpan.FromSeconds(20));

        }


        [ActivityDescription(Names.Activity.Test.Version, Name = Names.Activity.Test.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TestActivityWithInput : Activity
        {
            [ActivityMethod]
            public string Execute()
            {
                return "result";
            }
        }
    }
}