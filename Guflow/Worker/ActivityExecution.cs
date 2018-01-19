// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    /// <summary>
    /// Represent activity concurrent execution strategy
    /// </summary>
    public class ActivityExecution
    {
        private readonly uint _maximumLimit;
        private readonly Func<WorkerTask, Task> _executeFunc;
        private ActivityHost _activityHost;
        private readonly AsyncAutoResetEvent _completedEvent = new AsyncAutoResetEvent();
        private readonly object _syncObject=  new object();
        private volatile int _totalRunningTasks = 0;
        private volatile bool _reachedLimit = false;
        private ActivityExecution(uint maximumLimit)
        {
            if (maximumLimit > 1)
                _executeFunc = ExecuteConcurrentlyAsync;
            else
                _executeFunc = ExecuteInSequenceSync;

            _maximumLimit = maximumLimit;
        }

        /// <summary>
        /// Allow only one activity task to be executed.
        /// </summary>
        public static readonly ActivityExecution Sequencial = new ActivityExecution(1);

        /// <summary>
        /// Allow configured number of activity task to executed in parallel.
        /// </summary>
        /// <param name="maximumLimit"></param>
        /// <returns></returns>
        public static ActivityExecution Concurrent(uint maximumLimit)
        {
            Ensure.That(maximumLimit != 0, () => new ArgumentException(Resources.Concurrent_execution_limit_should_be_more_than_zero, "maximumLimit"));
            return new ActivityExecution(maximumLimit);
        }

        internal async Task ExecuteAsync(WorkerTask workerTask)
        {
            await _executeFunc(workerTask);
        }

        internal void Set(ActivityHost activityHost)
        {
            _activityHost = activityHost;
        }

        private async Task ExecuteConcurrentlyAsync(WorkerTask workerTask)
        {
            _reachedLimit = false;
            var task = Task.Run(async () =>
            {
                await ExecuteInSequenceSync(workerTask);
                ExecutionCompleted();
            });
            await WaitIfLimitHasReached();
        }

        private async Task WaitIfLimitHasReached()
        {
            lock (_syncObject)
            {
                _totalRunningTasks++;
                if (_totalRunningTasks >= _maximumLimit)
                {
                    _reachedLimit = true;
                }
            }
            if (_reachedLimit)
                await _completedEvent.WaitAsync();
        }

        private void ExecutionCompleted()
        {
            lock (_syncObject)
            {
                _totalRunningTasks--;
                if(_reachedLimit)
                    _completedEvent.Set();
            }
        }

        private async Task ExecuteInSequenceSync(WorkerTask workerTask)
        {
            try
            {
                var response = await workerTask.ExecuteFor(_activityHost);
                await _activityHost.SendAsync(response);
            }
            catch (Exception exception)
            {
               _activityHost.Fault(exception);
            }
        }
    }
}