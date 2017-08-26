﻿using System;
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

        public JumpWorkflowAction ToActivity(string name, string version, string positionalName = "")
        {
            Ensure.NotNullAndEmpty(name, "name");
            Ensure.NotNullAndEmpty(version, "version");

            var activityItem = _workflowItems.ActivityItemFor(Identity.New(name, version, positionalName));
            return WorkflowAction.JumpTo(activityItem).WithTriggerAction(_triggeringAction(activityItem));
        }
        public JumpWorkflowAction ToActivity<TActivity>(string positionalName = "") where TActivity: Activity
        {
            var description = ActivityDescriptionAttribute.FindOn<TActivity>();
            return ToActivity(description.Name, description.Version, positionalName);
        }
        public JumpWorkflowAction ToTimer(string name)
        {
            Ensure.NotNullAndEmpty(name, "name");
            var timerItem = _workflowItems.TimerItemFor(Identity.Timer(name));
            return WorkflowAction.JumpTo(timerItem).WithTriggerAction(_triggeringAction(timerItem));
        }
    }
}