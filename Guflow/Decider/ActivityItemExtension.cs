using System;
using Guflow.Properties;
using Newtonsoft.Json.Linq;

namespace Guflow.Decider
{
    public static class ActivityItemExtension
    {
        public static dynamic Result(this IActivityItem activityItem)
        {
            Ensure.NotNull(activityItem, "activityItem");
            var completedEvent = activityItem.LastEvent;
            var activityCompletedEvent = completedEvent as ActivityCompletedEvent;
            if(activityCompletedEvent == null)
                throw new InvalidOperationException(string.Format(Resources.Activity_result_can_not_accessed,
                                                    typeof(ActivityCompletedEvent), completedEvent!=null? completedEvent.GetType().ToString(): "Unkown"));
            return activityCompletedEvent.Result.FromJson();
        }

        public static TType Result<TType>(this IActivityItem activityItem)
        {
            Ensure.NotNull(activityItem, "activityItem");
            var completedEvent = activityItem.LastEvent;
            var activityCompletedEvent = completedEvent as ActivityCompletedEvent;
            if (activityCompletedEvent == null)
                throw new InvalidOperationException(string.Format(Resources.Activity_result_can_not_accessed,
                                                    typeof(ActivityCompletedEvent), completedEvent != null ? completedEvent.GetType().ToString() : "Unkown"));
            try
            {
                if (typeof(TType).Primitive())
                    return (TType)Convert.ChangeType(activityCompletedEvent.Result, typeof(TType));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(string.Format(Resources.Can_not_deserialize_json_data_into_type, activityCompletedEvent.Result, typeof(TType)), exception);
            }
            return activityCompletedEvent.Result.FromJson<TType>();
        }

        public static bool HasCompleted(this IActivityItem activityItem)
        {
            Ensure.NotNull(activityItem, "activityItem");
            return activityItem.LastEvent is ActivityCompletedEvent;
        }
    }
}