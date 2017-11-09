using System.Collections.Generic;
using System.Threading.Tasks;

namespace Guflow.Worker
{
    public class AsyncAutoResetEvent
    {
        private static readonly Task Completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;

        public Task WaitAsync()
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    _signaled = false;
                    return Completed;
                }
                var tcs = new TaskCompletionSource<bool>();
                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (_waits)
            {
                if (_waits.Count > 0)
                    toRelease = _waits.Dequeue();
                else if (!_signaled)
                    _signaled = true;
            }
            toRelease?.SetResult(true);
        }
    }
}