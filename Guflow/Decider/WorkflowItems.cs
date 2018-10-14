// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    internal class WorkflowItems
    {
        private readonly HashSet<WorkflowItem> _workflowItems = new HashSet<WorkflowItem>();

        public bool Add(WorkflowItem workflowItem)
        {
            return _workflowItems.Add(workflowItem);
        }
        public ActivityItem ActivityItem(WorkflowItemEvent workflowItemEvent)
        {
            var workflowActivity = Activity(workflowItemEvent);

            if (workflowActivity == null)
                throw new IncompatibleWorkflowException(string.Format("Can not find activity for event {0}.", workflowItemEvent));

            return workflowActivity;
        }
        public ActivityItem ActivityItem(Identity identity)
        {
            var workflowActivity = Activity(identity);

            if (workflowActivity == null)
                throw new WorkflowItemNotFoundException(
                    $"Can not find activity by {identity}.");
            return workflowActivity;
        }
        public TimerItem TimerItem(Identity identity)
        {
            var workflowTimer = TimerOf(identity);
            if (workflowTimer == null)
                throw new WorkflowItemNotFoundException($"Can not find timer by {identity}.");
            return workflowTimer;
        }

        public LambdaItem LambdaItem(Identity identity)
        {
            var lambdaItem = Lambda(identity);
            if (lambdaItem == null)
                throw new WorkflowItemNotFoundException($"Can not find lambda by {identity}.");
            return lambdaItem;
        }

        public ChildWorkflowItem ChildWorkflowItem(Identity identity)
        {
            var item = ChildWorkflow(identity);
            if(item == null)
                throw new WorkflowItemNotFoundException($"Can not find the child workflow by {identity}");
            return item;
        }
        public ITimer Timer(WorkflowItemEvent workflowItemEvent)
        {
            var timer = _workflowItems.FirstOrDefault(workflowItemEvent.IsFor) as ITimer;

            if (timer == null)
                throw new IncompatibleWorkflowException($"Can not find timer for event {workflowItemEvent}");

            return timer;
        }
        public TimerItem TimerItem(WorkflowItemEvent timerEvent)
        {
            return _workflowItems.OfType<TimerItem>().FirstOrDefault(timerEvent.IsFor);
        }
        public WorkflowItem WorkflowItem(WorkflowItemEvent workflowItemEvent)
        {
            var workflowItem = _workflowItems.FirstOrDefault(workflowItemEvent.IsFor);

            if (workflowItem == null)
                throw new IncompatibleWorkflowException($"Can not find workflow item for event {workflowItemEvent}");

            return workflowItem;
        }
        public WorkflowItem WorkflowItem(Identity identity)
        {
            return _workflowItems.FirstOrDefault(i => i.Has(identity));
        }

        public IEnumerable<WorkflowItem> StartupItems()
        {
            return _workflowItems.Where(i => i.IsStartupItem());
        }

        public IEnumerable<WorkflowItem> Childeren(WorkflowItem workflowItem)
        {
            return _workflowItems.Where(i => i.IsChildOf(workflowItem));
        }
        public ActivityItem Activity(WorkflowItemEvent activityEvent)
        {
            return _workflowItems.OfType<ActivityItem>().FirstOrDefault(activityEvent.IsFor);
        }
        public IEnumerable<IWorkflowItem> AllItems
        {
            get { return _workflowItems.Where(i=>i.GetType()!=typeof(WorkflowActionItem)); }
        }

        public LambdaItem LambdaItem(WorkflowItemEvent @event)
        {
            var item = _workflowItems.OfType<LambdaItem>().FirstOrDefault(@event.IsFor);
            if(item == null)
                throw new IncompatibleWorkflowException($"Can not find workflow item for event {@event}");
            return item;
        }

        public IEnumerable<IActivityItem> AllActivities => _workflowItems.OfType<ActivityItem>();

        public IEnumerable<ITimerItem> AllTimers => _workflowItems.OfType<TimerItem>();

        public IEnumerable<ILambdaItem> AllLambdas => _workflowItems.OfType<LambdaItem>();

        private TimerItem TimerOf(Identity identity)
        {
            return _workflowItems.OfType<TimerItem>().FirstOrDefault(s => s.Has(identity));
        }
        private ActivityItem Activity(Identity identity)
        {
            return _workflowItems.OfType<ActivityItem>().FirstOrDefault(a => a.Has(identity));
        }
        private LambdaItem Lambda(Identity identity)
        {
            return _workflowItems.OfType<LambdaItem>().FirstOrDefault(s => s.Has(identity));
        }

        private ChildWorkflowItem ChildWorkflow(Identity identity)
        {
            return _workflowItems.OfType<ChildWorkflowItem>().FirstOrDefault(w => w.Has(identity));
        }
        public ChildWorkflowItem ChildWorkflowItem(WorkflowItemEvent @event)
        {
            var item = _workflowItems.OfType<ChildWorkflowItem>().FirstOrDefault(@event.IsFor);
            if(item == null) throw new IncompatibleWorkflowException($"Can not find workflow item for event {@event}");

            return item;
        }
    }
}