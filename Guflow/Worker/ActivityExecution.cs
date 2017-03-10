using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class ActivityExecution
    {
        private readonly uint _maximumLimit;
        private readonly Func<WorkerTask, Task> _executeFunc;
        private HostedActivities _hostedActivities;
        private readonly ConcurrentDictionary<Task,string> _runningTasks = new ConcurrentDictionary<Task, string>();
        private ActivityExecution(uint maximumLimit)
        {
            if (maximumLimit > 1)
                _executeFunc = ExecuteConcurrently;
            else
                _executeFunc = ExecuteInSequence;

            _maximumLimit = maximumLimit;
        }

        public static readonly ActivityExecution Sequencial  = new ActivityExecution(1);

        public static ActivityExecution Concurrent(uint maximumLimit)
        {
            Ensure.That(maximumLimit != 0, ()=> new ArgumentException(Resources.Concurrent_execution_limit_should_be_than_zero, "maximumLimit"));
            return new ActivityExecution(maximumLimit);
        }

        internal async Task ExecuteAsync(WorkerTask workerTask)
        {
            await _executeFunc(workerTask);
        }

        internal void Set(HostedActivities hostedActivities)
        {
            _hostedActivities = hostedActivities;
        }

        private async Task ExecuteConcurrently(WorkerTask workerTask)
        {
            var task = new Task(async () =>await ExecuteInSequence(workerTask));
            
            var exceptionTask = task.ContinueWith(t => Environment.FailFast(Resources.Unhandled_activity_exception, t.Exception),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
            var adjustCountTask = task.ContinueWith(t =>
            {
                string value;
                if (!_runningTasks.TryRemove(t, out value))
                        Environment.FailFast(Resources.Activity_execution_count_mismatch);
            }, TaskContinuationOptions.ExecuteSynchronously);

            _runningTasks.TryAdd(task, "any_dummy_value");

            task.Start();

            if (_runningTasks.Count >= _maximumLimit)
                await Task.WhenAny(_runningTasks.Keys);
        }
        private async Task ExecuteInSequence(WorkerTask workerTask)
        {
            var response = await workerTask.ExecuteFor(_hostedActivities);
            await _hostedActivities.SendAsync(response);
        }
    }
}