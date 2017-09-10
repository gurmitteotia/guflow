﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Guflow.Properties;

namespace Guflow.Worker
{
    public class ActivityExecution
    {
        private readonly uint _maximumLimit;
        private readonly Func<WorkerTask, Task> _executeFunc;
        private HostedActivities _hostedActivities;
        private readonly AsyncAutoResetEvent _completedEvent = new AsyncAutoResetEvent();
        private int _totalRunningTasks = 0;
        private ActivityExecution(uint maximumLimit)
        {
            if (maximumLimit > 1)
                _executeFunc = ExecuteConcurrentlyAsync;
            else
                _executeFunc = ExecuteInSequenceSync;

            _maximumLimit = maximumLimit;
        }

        public static readonly ActivityExecution Sequencial = new ActivityExecution(1);

        public static ActivityExecution Concurrent(uint maximumLimit)
        {
            Ensure.That(maximumLimit != 0, () => new ArgumentException(Resources.Concurrent_execution_limit_should_be_more_than_zero, "maximumLimit"));
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

        private async Task ExecuteConcurrentlyAsync(WorkerTask workerTask)
        {
            var totalRunningTasks = Interlocked.Increment(ref _totalRunningTasks);
            var task = Task.Run(async () =>
            {

                await ExecuteInSequenceSync(workerTask);
                var remainRunningTasks = Interlocked.Decrement(ref _totalRunningTasks);
                if (remainRunningTasks < _maximumLimit)
                    _completedEvent.Set();
            });
            if (totalRunningTasks >= _maximumLimit)
                await _completedEvent.WaitAsync();
        }

        private async Task ExecuteInSequenceSync(WorkerTask workerTask)
        {
            try
            {
                var response = await workerTask.ExecuteFor(_hostedActivities);
                await _hostedActivities.SendAsync(response);
            }
            catch (Exception exception)
            {
               _hostedActivities.Fault(exception);
            }
        }
    }
}