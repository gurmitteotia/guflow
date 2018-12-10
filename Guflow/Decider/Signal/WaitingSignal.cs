// /Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root folder for license information.

namespace Guflow.Decider
{
    /// <summary>
    /// Expose APIs for waiting signals
    /// </summary>
    public class WaitingSignal
    {
        private readonly string _signalName;

        internal WaitingSignal(string signalName)
        {
            _signalName = signalName;
        }

        /// <summary>
        /// Resume the execution after waiting signal. If this signal is not waited upon then empty workflow action will be returned. If more than one workflow branches
        /// are waiting for this signal then one them will begin execution.
        /// </summary>
        /// <returns></returns>
        public WorkflowAction Resume()
        {
            var item = WaitingItem();
            return WorkflowAction.ContinueWorkflow((WorkflowItem)item);
        }

        /// <summary>
        /// Returns the waiting workflow item
        /// </summary>
        /// <returns></returns>
        public IWorkflowItem WaitingItem()
        {
            return null;
            //var waitingEvent = _historyEvents.SignalWaitingEvents();
        }
    }
}