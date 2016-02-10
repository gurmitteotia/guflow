using System;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    public class ActivityHost : IActivityHost
    {
        private readonly IActivityConnection _activityConnection;

        public ActivityHost(IActivityConnection activityConnection)
        {
            _activityConnection = activityConnection;
         }

        public void Open()
        {
            _activityConnection.PollForNewTask();
        }

        public void Shutdown()
        {
            


        }
      

        public void PolledTaskReturned(ActivityTask polledActivityTask)
        {
            
        }

        public bool HandleError(Exception exception)
        {
            return false;
        }

        public void ActivityFinished(ActivityResponse activityResponse)
        {
            _activityConnection.SendResponse(activityResponse);
        }
    }

    internal class ActivityTaskExecutioner : IActivityTaskExecutioner
    {
        private readonly IActivityHost _activityHost;
        private readonly ActivityFactory _activityFactory;
        private readonly ConcurrentControl _concurrentControl;

        public ActivityTaskExecutioner(IActivityHost activityHost, ActivityFactory activityFactory, ConcurrentControl concurrentControl)
        {
            _activityHost = activityHost;
            _activityFactory = activityFactory;
            _concurrentControl = concurrentControl;
        }

        public void Execute(ActivityTask polledActivityTask)
        {
            var activity = _activityFactory.Create(polledActivityTask);
            activity.Completed+=  (r)=>_activityHost.ActivityFinished(r);
            _concurrentControl.Execute(activity);
        }
    }

    internal class ConcurrentControl
    {
        public void Execute(IActivity activity)
        {
            throw new NotImplementedException();
        }
    }

    internal class ActivityFactory
    {
        public IActivity Create(ActivityTask activityTask)
        {
            IActivity activity = null;
            activity.SetActivityTask(activityTask);
            return activity;
        }
    }

    internal class ActivityConnection : IActivityConnection
    {
        public IPollingOperation PollForNewTask()
        {
            throw new NotImplementedException();
        }

        public void SendResponse(ActivityResponse activityResponse)
        {
            throw new NotImplementedException();
        }
    }

    internal class ActivityTaskPoller : IActivityTaskPoller
    {
        private readonly IActivityHost _activityHost;
        private readonly IActivityConnection _activityConnection;
        private IPollingOperation _currentPollingOperation;

        public ActivityTaskPoller(IActivityHost activityHost, IActivityConnection activityConnection)
        {
            _activityHost = activityHost;
            _activityConnection = activityConnection;
        }

        public void PollForNewTask()
        {
           _currentPollingOperation = _activityConnection.PollForNewTask();
            _currentPollingOperation.Invoke(NewTaskReturned, _activityHost.HandleError);
        }

        private void NewTaskReturned(ActivityTask newask)
        {
            _activityHost.PolledTaskReturned(newask);
        }

        public void StopPolling()
        {
            _currentPollingOperation.Stop();
        }
    }

    public interface IActivityConnection
    {
        IPollingOperation PollForNewTask();
        void SendResponse(ActivityResponse activityResponse);
    }

    public interface IPollingOperation
    {
        void Stop();
        void Invoke(Action<ActivityTask> newTaskReturned, Func<Exception, bool> handleError);
    }

    internal class ActivityPolledTask
    {
    }
}