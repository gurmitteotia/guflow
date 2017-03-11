
using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    internal class WorkerTask
    {
        private readonly ActivityTask _activityTask;
        private readonly Func<HostedActivities, Task<ActivityResponse>> _execute;
        public static readonly WorkerTask Empty = new WorkerTask();

        private WorkerTask()
        {
            _execute = (a) => Task.FromResult(ActivityResponse.Defer);
        }
        private WorkerTask(ActivityTask activityTask)
        {
            _activityTask = activityTask;
            _execute = ExecuteActivityTask;
        }

        public static WorkerTask CreateFor(ActivityTask activityTask)
        {
            return new WorkerTask(activityTask);
        }

        public async Task<ActivityResponse> ExecuteFor(HostedActivities hostedActivities)
        {
            return await _execute(hostedActivities);
        }

        private async Task<ActivityResponse> ExecuteActivityTask(HostedActivities hostedActivities)
        {
            var activity = hostedActivities.FindBy(_activityTask.ActivityType.Name, _activityTask.ActivityType.Version);
            var activityArgs = new ActivityArgs(_activityTask.Input,
                                                _activityTask.ActivityId,
                                                _activityTask.WorkflowExecution.WorkflowId,
                                                _activityTask.WorkflowExecution.RunId,
                                                _activityTask.TaskToken);
            activityArgs.StartedEventId = _activityTask.StartedEventId;
            return await activity.ExecuteAsync(activityArgs);
        }
    }
}