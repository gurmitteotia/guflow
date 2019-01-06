// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent the event of a scheduleable item like- activity, timer etc.
    /// </summary>
    public abstract class WorkflowItemEvent : WorkflowEvent
    {
        protected ScheduleId ScheduleId;
        internal bool IsFor(WorkflowItem workflowItem)
        {
            return workflowItem.Has(ScheduleId);
        }

        protected WorkflowItemEvent(long eventId)
            : base(eventId)
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
        /// Wait for the signal indefintely
        /// </summary>
        /// <param name="signalName">Signal name. Cases are ignored when comparing the signal names.</param>
        /// <returns></returns>
        public WaitForSignalsWorkflowAction WaitForSignal(string signalName)
        {
            Ensure.NotNullAndEmpty(signalName,nameof(signalName));
            return new WaitForSignalsWorkflowAction(ScheduleId, EventId, SignalWaitType.Any, signalName);
        }

        /// <summary>
        /// Wait for any signal indefinitly
        /// </summary>
        /// <param name="signalName"></param>
        /// <param name="signalNames"></param>
        /// <returns></returns>
        public WaitForSignalsWorkflowAction WaitForAnySignal(string signalName, params string[] signalNames)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new WaitForSignalsWorkflowAction(ScheduleId, EventId, SignalWaitType.Any, ValidEventNames(signalName, signalNames));
        }

        /// <summary>
        /// Wait for all signals indefinitly
        /// </summary>
        /// <param name="signalName"></param>
        /// <param name="signalNames"></param>
        /// <returns></returns>
        public WaitForSignalsWorkflowAction WaitForAllSignals(string signalName, params string[] signalNames)
        {
            Ensure.NotNullAndEmpty(signalName, nameof(signalName));
            return new WaitForSignalsWorkflowAction(ScheduleId, EventId, SignalWaitType.All, ValidEventNames(signalName, signalNames));
        }

        private static string[] ValidEventNames(string name,params string[] names)
            => new[]{ name }.Concat(names).Where(n=>!string.IsNullOrEmpty(n)).ToArray();
    }
}