using Amazon.SimpleWorkflow;
using Amazon.SimpleWorkflow.Model;

namespace NetPlayground
{
    internal static class HistoryEventsExtension
    {
        public static bool IsActivityCompletedEvent(this HistoryEvent historyEvent)
        {
            return historyEvent.EventType == EventType.ActivityTaskCompleted;
        }
    }
}