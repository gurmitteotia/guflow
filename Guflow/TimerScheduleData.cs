using System.Runtime.Serialization;

namespace Guflow
{
    [DataContract]
    internal class TimerScheduleData
    {
        [DataMember] public string TimerName;
        [DataMember] public bool IsARescheduleTimer;
    }
}