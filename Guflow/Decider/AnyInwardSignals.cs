﻿using System.Linq;

namespace Guflow.Decider
{
    /// <summary>
    /// Represent any inward signals.
    /// </summary>
    public class AnyInwardSignals
    {
        private readonly InwardSignal [] _signals;
        internal AnyInwardSignals(string[] eventNames, IWorkflow workflow)
        {
            _signals = eventNames.Select(e => new InwardSignal(e, workflow)).ToArray();
        }

        /// <summary>
        /// Returns true if any of the given signal has triggered the current execution.
        /// </summary>
        /// <returns></returns>
        public bool IsTriggered()
        {
            return _signals.Any(s => s.IsTriggered());
        }

        /// <summary>
        /// Returns true if any of the given signal has timed out.
        /// </summary>
        /// <returns></returns>
        public bool IsTimedout()
        {
            return _signals.Any(s => s.IsTimedout());
        }
    }
}