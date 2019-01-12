// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using Guflow.Properties;

namespace Guflow.Decider
{
    public class Signal
    {
        private readonly string _signalName;
        private readonly string _input;
        private readonly IWorkflow _workflow;
        internal Signal(string signalName, object input, IWorkflow workflow)
        {
            _signalName = signalName;
            _workflow = workflow;
            _input = input.ToAwsString();
        }
       
        /// <summary>
        /// Send signal to a workflow identified by workflowId and runId.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="runId"></param>
        /// <returns></returns>
        public WorkflowAction ForWorkflow(string workflowId, string runId)
        {
            Ensure.NotNullAndEmpty(workflowId,"workflowId");
            return WorkflowAction.Signal(_signalName, _input, workflowId, runId);
        }

        /// <summary>
        /// Reply to a signal if it was sent by a workflow. Throws exception if this signal was not sent by a workflow.
        /// </summary>
        /// <param name="workflowSignaledEvent"></param>
        /// <returns></returns>
        public WorkflowAction ReplyTo(WorkflowSignaledEvent workflowSignaledEvent)
        {
            Ensure.NotNull(workflowSignaledEvent, "workflowSignaledEvent");
            if(!workflowSignaledEvent.IsSentByWorkflow)
                throw new SignalException(Resources.Can_not_reply_to_signal);

            return WorkflowAction.Signal(_signalName, _input, workflowSignaledEvent.ExternalWorkflowId, workflowSignaledEvent.ExternalWorkflowRunid);
        }

        /// <summary>
        /// Send signal to child workflow.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public WorkflowAction ForChildWorkflow(string name, string version, string positionalName="")
        {
            Ensure.NotNull(name, nameof(name));
            Ensure.NotNull(version, nameof(version));
            var item = _workflow.ChildWorkflowItem(Identity.New(name, version, positionalName));
            return item.SignalAction(_signalName, _input);
        }

        /// <summary>
        /// Send signal to child workflow.
        /// </summary>
        /// <typeparam name="TWorkflow"></typeparam>
        /// <param name="positionalName"></param>
        /// <returns></returns>
        public WorkflowAction ForChildWorkflow<TWorkflow>(string positionalName = "") where TWorkflow : Workflow
        {
            var desc = WorkflowDescription.FindOn<TWorkflow>();
            return ForChildWorkflow(desc.Name, desc.Version, positionalName);
        }
        /// <summary>
        /// Send signal to child workflow.
        /// </summary>
        /// <param name="childWorkflow"></param>
        /// <returns></returns>
        public WorkflowAction ForChildWorkflow(IChildWorkflowItem childWorkflow)
        {
            Ensure.NotNull(childWorkflow, nameof(childWorkflow));
            return ForChildWorkflow(childWorkflow.Name, childWorkflow.Version, childWorkflow.PositionalName);
        }

        /// <summary>
        /// Return true if the current execution is triggered by the specific signal.
        /// </summary>
        /// <returns></returns>
        public bool IsTriggered()
        {
            var signaledEvent = _workflow.CurrentlyExecutingEvent as WorkflowSignaledEvent;
            if (signaledEvent == null) return false;
            return string.Equals(signaledEvent.SignalName, _signalName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if the specific signal is received by this workflow. It will search entire workflow execution history. It will ignore the the case when looking for this signal.
        /// </summary>
        /// <returns></returns>
        public bool IsReceived()
        {
            var historyEvents = _workflow.WorkflowHistoryEvents;
            foreach (var signalEvent in historyEvents.AllSignalEvents())
            {
                if (string.Equals(_signalName, signalEvent.SignalName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}