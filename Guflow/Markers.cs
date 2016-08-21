using System.Collections.Generic;

namespace Guflow
{
    internal static class Markers
    {
        private static readonly List<RecordMarkerDecision> MarkerDecisions = new List<RecordMarkerDecision>(); 
        public static void Add(string markerName, object details)
        {
            MarkerDecisions.Add(new RecordMarkerDecision(markerName,details.ToAwsString()));
        }
        public static IEnumerable<RecordMarkerDecision> Decisions { get { return MarkerDecisions;} }

        public static void Clear()
        {
            MarkerDecisions.Clear();
        }
    }
}