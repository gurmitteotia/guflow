using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Decider;
using Guflow.Worker;
using NUnit.Framework;

namespace Guflow.IntegrationTests
{
    [TestFixture]
    public class DeflowAlgorithmTests
    {
        private WorkflowsHost _workflowsHost;
        private ActivitiesHost _activitiesHost;
        private TestDomain _domain;
        private static string _taskListName;

        [SetUp]
        public async Task Setup()
        {
            _domain = new TestDomain();
            _taskListName = Guid.NewGuid().ToString();
            _activitiesHost = await HostAsync(typeof(DownloadActivity), typeof(TranscodeActivity), typeof(SendEmailActivity));
        }

        [TearDown]
        public void TearDown()
        {
            _workflowsHost.StopExecution();
            _activitiesHost.StopExecution();
        }

        [Test]
        public async Task By_default_schedule_the_activity_with_workflow_input()
        {
            var @event = new ManualResetEvent(false);
            var workflow = new WorkflowWithMultipleParent();
            workflow.Closed += (s, e) => @event.Set();
            _workflowsHost = await HostAsync(workflow);

            await _domain.StartWorkflow<WorkflowWithMultipleParent>("input", _taskListName);
            @event.WaitOne();

            Assert.That(SendEmailActivity.Input.File1, Is.EqualTo("TranscodedPathMP4"));
            Assert.That(SendEmailActivity.Input.File2, Is.EqualTo("TranscodedPathAV"));
        }

        private async Task<WorkflowsHost> HostAsync(params Workflow[] workflows)
        {
            var hostedWorkflows = await _domain.Host(workflows);
            hostedWorkflows.StartExecution(new TaskList(_taskListName));
            return hostedWorkflows;
        }

        private async Task<ActivitiesHost> HostAsync(params Type[] activityTypes)
        {
            var hostedActivities = await _domain.Host(activityTypes);
            hostedActivities.StartExecution(new TaskList(_taskListName));
            return hostedActivities;
        }

        [WorkflowDescription(Names.Workflow.Test.Version, Name = Names.Workflow.Test.Name, DefaultChildPolicy = ChildPolicy.Abandon, DefaultExecutionStartToCloseTimeoutInSeconds = 900, DefaultTaskListName = "DefaultTaskList",
           DefaultTaskPriority = 10, DefaultTaskStartToCloseTimeoutInSeconds = 900, Description = "Empty workflow")]
        private class WorkflowWithMultipleParent : Workflow
        {
            public WorkflowWithMultipleParent()
            {
                ScheduleActivity<DownloadActivity>().OnTaskList((t) => _taskListName);

                ScheduleActivity<TranscodeActivity>("MP4").OnTaskList((t) => _taskListName)
                     .AfterActivity<DownloadActivity>().WithInput(a => new {Format = "MP4"});

                ScheduleActivity<TranscodeActivity>("AV").OnTaskList((t) => _taskListName)
                     .AfterActivity<DownloadActivity>().WithInput(a => new { Format = "AV" });

                ScheduleActivity<SendEmailActivity>()
                    .AfterActivity<TranscodeActivity>("MP4")
                    .AfterActivity<TranscodeActivity>("AV")
                    .OnTaskList(t=>_taskListName)
                    .WithInput(a => new
                    {
                        File1 = a.ParentActivity<TranscodeActivity>("MP4").Result(),
                        File2 = a.ParentActivity<TranscodeActivity>("AV").Result()
                    });
            }
        }


        [ActivityDescription(Names.Activity.Download.Version, Name = Names.Activity.Download.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
           DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class DownloadActivity : Activity
        {
            [ActivityMethod]
            public async Task<string> Execute()
            {
                await Task.Yield();
                return "DownloadedPath";
            }
        }

        [ActivityDescription(Names.Activity.Transcode.Version, Name = Names.Activity.Transcode.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
          DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class TranscodeActivity : Activity
        {
            private static readonly Random _random = new Random();
            [ActivityMethod]
            public async Task<string> Execute(TranscodeInput input)
            {
                await Task.Delay(_random.Next(100, 5000));
                return "TranscodedPath" + input.Format;
            }
        }

        [ActivityDescription(Names.Activity.SendEmail.Version, Name = Names.Activity.SendEmail.Name, DefaultTaskListName = "DefaultTaskList", DefaultTaskPriority = 10, Description = "some activity",
         DefaultHeartbeatTimeoutInSeconds = 10, DefaultScheduleToStartTimeoutInSeconds = 10, DefaultStartToCloseTimeoutInSeconds = 10, DefaultScheduleToCloseTimeoutInSeconds = 20)]
        private class SendEmailActivity : Activity
        {
            [ActivityMethod]
            public async Task<string> Execute(SendEmailInput input)
            {
                await Task.Yield();
                Input = input;
                return "SendEmail";
            }

            public static SendEmailInput Input;
        }

        private class SendEmailInput
        {
            public string File1;
            public string File2;
        }

        private class TranscodeInput
        {
            public string Format;
        }
    }
}