﻿// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SimpleWorkflow.Model;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the event of a scheduleable item like- activity, timer etc.
    /// </summary>
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        protected internal ScheduleId ScheduleId;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(ScheduleId);
        }

        protected WorkflowItemEvent(HistoryEvent historyEvent)
            : base(historyEvent)
        {
        }
        /// <summary>
        /// Indicate if this is an active event.
        /// </summary>
        public bool IsActive { get; protected set; }

        internal virtual bool InChainOf(IEnumerable<WorkflowItemEvent> workflowItemEvents)
        {
            return false;
        }

        internal bool HasSameScheduleId(WorkflowItemEvent other) => ScheduleId == other.ScheduleId;
        
        /// <summary>
        /// Pause the workflow execution until the specific signal is received. Workflow execution will continue when the specific signal is received.
        /// Signal name is case insensitive.
        /// </summary>
        /// <param name="signalName">Signal name the workflow will wait for.</param>
        /// <returns></returns>
        public WorkflowItemWaitAction WaitForSignal(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName,nameof(signalName));
            return new WorkflowItemWaitAction(this, SignalWaitType.Any, signalName);
        }

        /// <summary>
        /// Pause the workflow execution until any of the specific signal is received. Workflow execution will continue when either of the configured signal is received.
        /// Signal names are case insensitive
        /// </summary>
        /// <param name="signalName">Signal name the workflow will wait for.</param>
        /// <param name="signalNames">Optional signal names.</param>
        /// <returns></returns>
        public WorkflowItemWaitAction WaitForAnySignal(string signalName, params string[] signalNames)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new WorkflowItemWaitAction(this, SignalWaitType.Any, signalName.CombinedValidEventNames(signalNames));
        }

        /// <summary>
        /// Pause the workflow execution until all of the signals are received. Workflow execution will continue when all of the configured signals are received.
        /// Signal names are case insensitive
        /// </summary>
        /// <param name="signalName">Signal name the workflow will wait for.</param>
        /// <param name="signalNames">Optional signal names</param>
        /// <returns></returns>
        public WorkflowItemWaitAction WaitForAllSignals(string signalName, params string[] signalNames)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new WorkflowItemWaitAction(this, SignalWaitType.All, signalName.CombinedValidEventNames(signalNames));
        }
    }
}