using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class ConcurrentExecution
    {
        private readonly uint _maximumLimit;
        private readonly Func<WorkerTask, Task> _executeFunc;
        private HostedActivities _hostedActivities;
        private readonly ConcurrentDictionary<Task,string> _runningTasks = new ConcurrentDictionary<Task, string>();
        private ConcurrentExecution(uint maximumLimit)
        {
            if (maximumLimit > 1)
                _executeFunc = ExecuteConcurrently;
            else
                _executeFunc = ExecuteInSequence;

            _maximumLimit = maximumLimit;
        }
        public static ConcurrentExecution LimitTo(uint maximumLimit)
        {
            Ensure.That(maximumLimit != 0, ()=> new ArgumentException(Resources.Concurrent_execution_limit_should_be_than_zero, "maximumLimit"));
            return new ConcurrentExecution(maximumLimit);
        }

        internal async Task Execute(WorkerTask workerTask)
        {
            await _executeFunc(workerTask);
        }

        internal void Set(HostedActivities hostedActivities)
        {
            _hostedActivities = hostedActivities;
        }

        private async Task ExecuteConcurrently(WorkerTask workerTask)
        {
            var task = Task.Run(async () =>
            {
                await ExecuteInSequence(workerTask);

            }).ContinueWith(async t =>
            {
                string value;
                while (!_runningTasks.TryRemove(t, out value))
                    await Task.Yield();
            });

            _runningTasks.TryAdd(task, "any_dummy_value");

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