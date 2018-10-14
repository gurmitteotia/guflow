// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using Guflow.Worker;

namespace Guflow.Decider
{
    /// <summary>
    /// Provide APIs to support cancellation of activities, timer and workflows.
    /// </summary>
    public class CancelRequest
    {
        private readonly WorkflowItems _workflowItems;
        internal CancelRequest(WorkflowItems workflowItems)
        {
            _workflowItems = workflowItems;
        }
        /// <summary>
        /// Returns the workflow action to cancel the given activity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public  WorkflowAction ForActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name,"name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItem(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(activityItem);
        }
        /// <summary>
        /// Returns the workflow action to cancel the TActivity with given positionalName.
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public WorkflowAction ForActivity<TActivity>(string positionalName = "") where TActivity: Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return ForActivity(description.Name, description.Version, positionalName);
        }
        /// <summary>
        /// Returns the workflow action to cancel the timer.
        /// </summary>
        /// <param name="timerName"></param>
        /// <returns></returns>
        public WorkflowAction ForTimer(string timerName)
        {
            Ensure.NotNullAndEmpty(timerName, "timerName");

            var timerItem = _workflowItems.TimerItem(Identity.Timer(timerName));
            return WorkflowAction.Cancel(timerItem);
        }
        /// <summary>
        /// Returns the workflow action to cancel running workflow.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="runId"></param>
        /// <returns></returns>
        public WorkflowAction ForWorkflow(string workflowId, string runId = null)
        {
            Ensure.NotNullAndEmpty(workflowId, "workflowId");
            return WorkflowAction.CancelWorkflowRequest(workflowId, runId);
        }
        /// <summary>
        /// Returns the workflow action to cancel all given workflow items- activities, timers.
        /// </summary>
        /// <param name="workflowItems"></param>
        /// <returns></returns>
        public WorkflowAction For(IEnumerable<IWorkflowItem> workflowItems)
        {
            Ensure.NotNull(workflowItems, "workflowItems");
            return WorkflowAction.Cancel(workflowItems.OfType<WorkflowItem>());
        }
        /// <summary>
        /// Return the workflow to cancel all given workflow items.
        /// </summary>
        /// <param name="workflowItems"></param>
        /// <returns></returns>
        public WorkflowAction For(params IWorkflowItem[] workflowItems)
        {
            Ensure.NotNull(workflowItems, "workflowItems");
            return For(workflowItems.AsEnumerable());
        }

        /// <summary>
        /// Returns workflow action to cancel the child workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public WorkflowAction ForChildWorkflow(string name, string version, string positionalName="")
        {
            Ensure.NotNullAndEmpty(name, nameof(name));
            Ensure.NotNullAndEmpty(version, nameof(version));
            var item = _workflowItems.ChildWorkflowItem(Identity.New(name, version, positionalName));
            return WorkflowAction.Cancel(item);
        }

        /// <summary>
        /// Returns workflow action to cancel the child workflow.
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public WorkflowAction ForChildWorkflow<TWorkflow>(string positionalName="") where TWorkflow : Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return ForChildWorkflow(desc.Name, desc.Version, positionalName);
        }
    }
}