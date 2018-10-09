// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Threading.Tasks;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    internal class WorkerTask
    {
        private readonly ActivityTask _activityTask;
        private readonly IHeartbeatSwfApi _heartbeatSwfApi;
        private readonly Func<IHostedActivities, Task<ActivityResponse>> _execute;
        private IErrorHandler _errorHandler;
        public static readonly WorkerTask Empty = new WorkerTask();

        private WorkerTask()
        {
            _execute = (a) => Task.FromResult(ActivityResponse.Defer);
            Token = "INVALID_TOKEN";
        }
        private WorkerTask(ActivityTask activityTask, IHeartbeatSwfApi heartbeatSwfApi, IErrorHandler errorHandler)
        {
            _activityTask = activityTask;
            _heartbeatSwfApi = heartbeatSwfApi;
            _errorHandler = errorHandler;
            _execute = ExecuteActivityTaskAsync;
            Token = activityTask.TaskToken;
        }

        public string Token { get;}

        public static WorkerTask CreateFor(ActivityTask activityTask, IHeartbeatSwfApi heartbeatSwfApi)
        {
            return new WorkerTask(activityTask,heartbeatSwfApi , ErrorHandler.Default(e=>ErrorAction.Unhandled));
        }

        public async Task<ActivityResponse> ExecuteFor(IHostedActivities hostedActivities)
        {
            return await _execute(hostedActivities);
        }

        private async Task<ActivityResponse> ExecuteActivityTaskAsync(IHostedActivities hostedActivities)
        {
            var activity = hostedActivities.FindBy(_activityTask.ActivityType.Name, _activityTask.ActivityType.Version);
            var activityArgs = new ActivityArgs(_activityTask.Input,
                                                _activityTask.ActivityId,
                                                _activityTask.WorkflowExecution.WorkflowId,
                                                _activityTask.WorkflowExecution.RunId,
                                                _activityTask.TaskToken);
            activityArgs.StartedEventId = _activityTask.StartedEventId;
            activity.SetErrorHandler(_errorHandler);
            activity.SetSwfApi(_heartbeatSwfApi);
            return await activity.ExecuteAsync(activityArgs);
        }

        public void SetErrorHandler(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }
    }
}