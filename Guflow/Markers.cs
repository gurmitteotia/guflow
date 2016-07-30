using System.Collections.Generic;
using System.Linq;

namespace Guflow
{
    public class Markers
    {
        private readonly Dictionary<string,string> _markers = new Dictionary<string, string>();
        private readonly IWorkflow _workflow;
        internal Markers(IWorkflow workflow)
        {
            _workflow = workflow;
        }
        public void Add(string name, object details)
        {
            var awsDetails = details.ToAwsString();
            _markers.Add(name,awsDetails);
        }
        public IEnumerable<MarkerRecordedEvent> AllRecordedEvents
        {
            get {return _workflow.CurrentHistoryEvents.AllMarkerRecordedEvents(); }
        } 

        internal IEnumerable<RecordMarkerDecision> GetDecisions()
        {
            return _markers.Select(kv => new RecordMarkerDecision(kv.Key, kv.Value));
        }
        internal void Clear()
        {
            _markers.Clear();
        }
    }
}