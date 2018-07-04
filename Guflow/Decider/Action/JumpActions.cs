// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
using System;
using Guflow.Worker;

namespace Guflow.Decider
{
    public sealed class JumpActions
    {
        private readonly WorkflowItems _workflowItems;
        private readonly Func<WorkflowItem, WorkflowAction> _triggeringAction;

        private JumpActions(WorkflowItems workflowItems, Func<WorkflowItem, WorkflowAction> triggeringAction)
        {
            _workflowItems = workflowItems;
            _triggeringAction = triggeringAction;
        }

        internal static JumpActions FromWorkflowItem(WorkflowItems workflowItems, WorkflowItem workflowItem)
        {
            return new JumpActions(workflowItems, jumpItem=>new TriggerActions(workflowItem).FirstJoint(jumpItem));
        }

        internal static JumpActions FromWorkflowEvent(WorkflowItems workflowItems)
        {
            return new JumpActions(workflowItems, jumpItem => WorkflowAction.Empty);
        }

        /// <summary>
        /// Jump to an activity. Exception is thrown if activity is already active or not found in workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public JumpWorkflowAction ToActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItem(Identity.New(name, version, positionalName));
            return WorkflowAction.JumpTo(activityItem).WithTriggerAction(_triggeringAction(activityItem));
        }
        /// <summary>
        /// Jump to an activity. Exception is thrown if activity is already active or not found in workflow. It reads activity name and version
        /// from <see cref="ActivityDescriptionAttribute"/> of TActivity.
        /// </summary>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public JumpWorkflowAction ToActivity<TActivity>(string positionalName = "") where TActivity: Activity
        {
            var description = ActivityDescription.FindOn<TActivity>();
            return ToActivity(description.Name, description.Version, positionalName);
        }
        /// <summary>
        /// Jump to workflow timer. Exceptio is thrown if timer is already active or not found in workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JumpWorkflowAction ToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");
            var timerItem = _workflowItems.TimerItem(Identity.Timer(name));
            return WorkflowAction.JumpTo(timerItem).WithTriggerAction(_triggeringAction(timerItem));
        }
    }
}