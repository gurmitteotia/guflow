
using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    internal class WorkerTask
    {
        private readonly ActivityTask _activityTask;
        private readonly Func<ActivityHost, Task<ActivityResponse>> _execute;
        private IErrorHandler _errorHandler;
        public static readonly WorkerTask Empty = new WorkerTask();

        private WorkerTask()
        {
            _execute = (a) => Task.FromResult(ActivityResponse.Defer);
        }
        private WorkerTask(ActivityTask activityTask, IErrorHandler errorHandler)
        {
            _activityTask = activityTask;
            _errorHandler = errorHandler;
            _execute = ExecuteActivityTask;
        }

        public static WorkerTask CreateFor(ActivityTask activityTask)
        {
            return new WorkerTask(activityTask, ErrorHandler.Default(e=>ErrorAction.Unhandled));
        }

        public async Task<ActivityResponse> ExecuteFor(ActivityHost activityHost)
        {
            return await _execute(activityHost);
        }

        private async Task<ActivityResponse> ExecuteActivityTask(ActivityHost activityHost)
        {
            var activity = activityHost.FindBy(_activityTask.ActivityType.Name, _activityTask.ActivityType.Version);
            var activityArgs = new ActivityArgs(_activityTask.Input,
                                                _activityTask.ActivityId,
                                                _activityTask.WorkflowExecution.WorkflowId,
                                                _activityTask.WorkflowExecution.RunId,
                                                _activityTask.TaskToken);
            activityArgs.StartedEventId = _activityTask.StartedEventId;
            activity.SetErrorHandler(_errorHandler);
            return await activity.ExecuteAsync(activityArgs);
        }

        public void SetErrorHandler(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }
    }
}