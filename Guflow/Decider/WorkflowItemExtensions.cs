// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    public static class WorkflowItemExtensions
    {
        public static IEnumerable<WorkflowItemEvent> AllEventsOf(this IWorkflowItem workflowItem, Type eventType)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.AllEvents.Where(e => e.GetType() == eventType);
        }

        public static IEnumerable<TEvent> AllEventsOf<TEvent>(this IWorkflowItem workflowItem) where TEvent : WorkflowItemEvent
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.AllEvents.OfType<TEvent>();
        }
        public static IActivityItem ParentActivity(this IWorkflowItem workflowItem, string name, string version, string positionalName = "")
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentActivities.OfType<ActivityItem>()
                    .FirstOrDefault(a => a.Has(Identity.New(name, version, positionalName)));
        }
        public static IActivityItem ParentActivity<TActivity>(this IWorkflowItem workflowItem, string positionalName = "") where TActivity: Activity
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return workflowItem.ParentActivity(description.Name, description.Version, positionalName);
        }
        public static IActivityItem ParentActivity(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentActivities.Single();
        }
        public static ITimerItem ParentTimer(this IWorkflowItem workflowItem, string name)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentTimers.OfType<TimerItem>()
                    .FirstOrDefault(a => a.Has(Identity.Timer(name)));
        }
        public static ITimerItem ParentTimer(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentTimers.Single();
        }
    }
}