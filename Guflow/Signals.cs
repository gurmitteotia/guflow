using System.Collections.Generic;

namespace Guflow
{
    internal static class Signals
    {
        private static readonly List<SignalWorkflowDecision> SignalDecisions = new List<SignalWorkflowDecision>();

        internal static Signal New(string signalName, object input)
        {
            return new Signal(signalName, input.ToAwsString(),SignalDecisions);
        }
       
        internal static IEnumerable<SignalWorkflowDecision> Decisions { get { return SignalDecisions; } }

        public static void Clear()
        {
            SignalDecisions.Clear();
        }
    }
}