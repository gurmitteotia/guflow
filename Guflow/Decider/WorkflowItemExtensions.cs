// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    public static class WorkflowItemExtensions
    {
        /// <summary>
        /// Return all events of a specific event type. e.g. ActivityFailedEvent, ActivityTimedoutEvent
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <param name="eventType"></param>
        /// <param name="includeRescheduleTimerEvents"></param>
        /// <returns></returns>
        public static IEnumerable<WorkflowItemEvent> Events(this IWorkflowItem workflowItem, Type eventType, bool includeRescheduleTimerEvents = false)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.AllEvents(includeRescheduleTimerEvents).Where(e => e.GetType() == eventType);
        }

        /// <summary>
        /// Return all events of a specific event type. e.g. ActivityFailedEvent, ActivityTimedoutEvent
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="workflowItem"></param>
        /// <param name="includeRescheduleTimerEvents"></param>
        /// <returns></returns>
        public static IEnumerable<TEvent> Events<TEvent>(this IWorkflowItem workflowItem, bool includeRescheduleTimerEvents = false) where TEvent : WorkflowItemEvent
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.AllEvents(includeRescheduleTimerEvents).OfType<TEvent>();
        }

        /// <summary>
        /// Returns parent activity by given parameters. Returns null not if not exists.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public static IActivityItem ParentActivity(this IWorkflowItem workflowItem, string name, string version, string positionalName = "")
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            Ensure.NotNull(name, nameof(name));
            Ensure.NotNull(version, nameof(version));
            return workflowItem.ParentActivities.OfType<ActivityItem>()
                    .FirstOrDefault(a => a.Has(Identity.New(name, version, positionalName)));
        }
        /// <summary>
        /// Returns parent activity. Returns null if not exists.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="workflowItem"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public static IActivityItem ParentActivity<TActivity>(this IWorkflowItem workflowItem, string positionalName = "") where TActivity : Activity
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            var description = ActivityDescription.FindOn<TActivity>();
            return workflowItem.ParentActivity(description.Name, description.Version, positionalName);
        }
        /// <summary>
        /// Return parent activity. Throws exception if more than one parent activity exists.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <returns></returns>
        public static IActivityItem ParentActivity(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentActivities.Single();
        }

        /// <summary>
        /// Returns the parent timer by given name. Returns null if not exists.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ITimerItem ParentTimer(this IWorkflowItem workflowItem, string name)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentTimers.OfType<TimerItem>()
                    .FirstOrDefault(a => a.Has(Identity.Timer(name)));
        }
        /// <summary>
        ///  Returns parent timer. Throws exception if more than one parent timer exists.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <returns></returns>
        public static ITimerItem ParentTimer(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentTimers.Single();
        }

        /// <summary>
        /// Find the parent lambda by given parameters. Returns null if not found.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <param name="name"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public static ILambdaItem ParentLambda(this IWorkflowItem workflowItem, string name,
            string positionalName = "")
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentLambdas.OfType<LambdaItem>()
                .FirstOrDefault(a => a.Has(Identity.Lambda(name, positionalName)));
        }

        /// <summary>
        ///  Returns parent lambda. Throws exception if more than one parent lambda exists.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <returns></returns>
        public static ILambdaItem ParentLambda(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, "workflowItem");
            return workflowItem.ParentLambdas.Single();
        }

        /// <summary>
        /// Returns the parent child-workflow by given parameter. Returns null if it can find the parent child workflow for given parameters.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public static IChildWorkflowItem ParentChildWorkflow(this IWorkflowItem workflowItem, string name, string version,
            string positionalName = "")
        {
            Ensure.NotNull(workflowItem, nameof(workflowItem));
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            return workflowItem.ParentChildWorkflows.OfType<ChildWorkflowItem>()
                .FirstOrDefault(a => a.Has(Identity.New(name, version, positionalName)));
        }

        /// <summary>
        /// Returns the parent child-workflow of item. Throws exception if none or more than one parent child-workflows are found.
        /// </summary>
        /// <param name="workflowItem"></param>
        /// <returns></returns>
        public static IChildWorkflowItem ParentChildWorkflow(this IWorkflowItem workflowItem)
        {
            Ensure.NotNull(workflowItem, nameof(workflowItem));
            return workflowItem.ParentChildWorkflows.OfType<ChildWorkflowItem>().Single();

        }
        /// <summary>
        /// Returns the parent child-workflow for <see cref="TWorkflow"/> and positional name. Returns null if not
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="workflowItem"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public static IChildWorkflowItem ParentChildWorkflow<TWorkflow>(this IWorkflowItem workflowItem, string positionalName = "")
        where TWorkflow : Workflow
        {
            Ensure.NotNull(workflowItem, nameof(workflowItem));
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return workflowItem.ParentChildWorkflow(desc.Name, desc.Version, positionalName);
        }
    }
}