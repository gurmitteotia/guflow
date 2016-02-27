using System.Runtime.Serialization;

namespace Guflow
{
    [DataContract]
    internal class TimerScheduleData
    {
        [DataMember] public string Identity;
        [DataMember] public bool IsARescheduleTimer;
    }
}