using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class ActivityFailureTests
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
            _activityHost = await HostAsync(typeof(FailingActivity));
            FailingActivity.ExecutionTimes = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _workflowHost.StopExecution();
            _activityHost.StopExecution();
        }

        [Test]
        public async Task By_default_failed_activity_fails_workflow()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowWithNoRetry();
            string reason = null;
            string details = null;
            workflow.Failed += (s, e) => { reason = e.Reason; details = e.Details; @event.Set(); };
            _workflowHost = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowWithNoRetry>("input", _taskListName);
            @event.WaitOne();

            Assert.That(reason, Is.EqualTo(typeof(IOException).Name));
            Assert.That(details, Is.EqualTo("Failed to write to disk."));
        }

        [Test]
        public async Task Failed_activity_can_be_retried_immediatly()
        {
            var @event = new ManualResetEvent(false);
            uint retryAttempts = 2;
            var workflow = new WorkflowToRetryActivityImmediately(retryAttempts);
            workflow.Failed += (s, e) =>  @event.Set();
            _workflowHost = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowToRetryActivityImmediately>("input", _taskListName);
            @event.WaitOne();

            Assert.That(FailingActivity.ExecutionTimes, Is.EqualTo(retryAttempts+1));
        }

        [Test]
        public async Task Failed_activity_can_be_retried_after_time_out()
        {
            var @event = new ManualResetEvent(false);
            uint retryAttempts = 2;
            var workflow = new WorkflowToRetryActivityAfterTimeout(retryAttempts);
            workflow.Failed += (s, e) => @event.Set();
            _workflowHost = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowToRetryActivityImmediately>("input", _taskListName);
            @event.WaitOne();

            Assert.That(FailingActivity.ExecutionTimes, Is.EqualTo(retryAttempts + 1));
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
        private class WorkflowWithNoRetry : Workflow
        {
            public WorkflowWithNoRetry()
            {
                ScheduleActivity("TestActivity", "1.0").OnTaskList((t) => _taskListName);
            }
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToRetryActivityImmediately : Workflow
        {
            public WorkflowToRetryActivityImmediately(uint retryAttempts)
            {
                ScheduleActivity<FailingActivity>().OnTaskList((t) => _taskListName)
                                                    .OnFailure(e => Reschedule(e).UpTo(Limit.Count(retryAttempts)));
            }
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
         DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowToRetryActivityAfterTimeout : Workflow
        {
            public WorkflowToRetryActivityAfterTimeout(uint retryAttempts)
            {
              ScheduleActivity<FailingActivity>().OnTaskList((t) => _taskListName)
                                                   .OnFailure(e => Reschedule(e).After(TimeSpan.FromSeconds(1)).UpTo(Limit.Count(retryAttempts)));
            }
        }

        [ActivityDescription(Names.Activity.Test.Version, Name = Names.Activity.Test.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        [EnableHeartbeat(IntervalInMilliSeconds = 5000)]
        private class FailingActivity : Activity
        {
            public static int ExecutionTimes = 0;
            [ActivityMethod]
            public string Execute(string input)
            {
                ExecutionTimes++;
                throw new IOException("Failed to write to disk.");
            }
        }
    }
}