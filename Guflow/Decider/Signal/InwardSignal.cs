// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

using System;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent a signal received by the workflow.
    /// </summary>
    public class InwardSignal
    {
        private readonly string _signalName;
        private readonly IWorkflow _workflow;

        internal InwardSignal(string signalName, IWorkflow workflow)
        {
            _signalName = signalName;
            _workflow = workflow;
        }


        /// <summary>
        /// Return true if the current execution is triggered by the specific signal.
        /// </summary>
        /// <returns></returns>
        public bool IsTriggered()=> IsTriggered(d => true);


        /// <summary>
        /// Return true if the current execution is triggered by specific signal and specific data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsTriggered(Func<string, bool> data)
        {
            var signaledEvent = _workflow.CurrentlyExecutingEvent as WorkflowSignaledEvent;
            if (signaledEvent == null) return false;
            return string.Equals(signaledEvent.SignalName, _signalName, StringComparison.OrdinalIgnoreCase) &&
                   data(signaledEvent.Input);
        }

        /// <summary>
        /// Returns true if the specific signal is received by this workflow. It will search entire workflow execution history using case insensitive approach.
        /// </summary>
        /// <returns></returns>
        public bool IsReceived() => IsReceived(d => true);
      

        /// <summary>
        /// Returns true if the specific signal with data is received by this workflow. It will search entire workflow execution history using case insensitive approach.
        /// </summary>
        /// <returns></returns>
        public bool IsReceived(Func<string, bool> data)
        {
            var historyEvents = _workflow.WorkflowHistoryEvents;
            foreach (var signalEvent in historyEvents.AllSignalEvents())
            {
                if (string.Equals(_signalName, signalEvent.SignalName, StringComparison.OrdinalIgnoreCase) && data(signalEvent.Input))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if the current execution is triggered because the given signal is timedout.
        /// </summary>
        /// <returns></returns>
        public bool IsTimedout()
        {
            var timerFiredEvent = _workflow.CurrentlyExecutingEvent as TimerFiredEvent;
            if (timerFiredEvent == null|| timerFiredEvent.TimerType!=TimerType.SignalTimer) return false;

            var signaledEvent = _workflow.TimedoutEvent(timerFiredEvent);
            return signaledEvent.IsTimedout(_signalName);
        }
    }
}