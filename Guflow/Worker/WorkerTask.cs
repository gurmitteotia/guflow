
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Worker
{
    internal class WorkerTask
    {
        private ActivityTask _activityTask;

        public static readonly WorkerTask Empty = new WorkerTask();

        private WorkerTask()
        {
        }
        private WorkerTask(ActivityTask activityTask)
        {
            _activityTask = activityTask;
        }

        public static WorkerTask CreateFor(ActivityTask activityTask)
        {
            return new WorkerTask(activityTask);
        }
    }
}