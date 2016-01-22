using System;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    internal interface IActivityHost
    {
        void PolledTaskReturned(ActivityTask polledActivityTask);

        bool HandleError(Exception exception);

        void ActivityFinished(ActivityResponse activityResponse);
    }

    public class ActivityResponse
    {

    }


    public interface IActivity
    {
        void SetActivityTask(ActivityTask activityTask);

        event Action<ActivityResponse> Completed;
    }

    public interface IActivityTaskExecutioner
    {
        void Execute(ActivityTask polledActivityTask);
    }
}