// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    /// <summary>
    /// Represent the identity for a scheduled workflow item. This class is used internally.
    /// </summary>
    public sealed class ScheduleId
    {
        private readonly string _id;
        private readonly Identity _itemId;

        private ScheduleId(Identity itemId, string id)
        {
            _id = id;
            _itemId = itemId;
        }

        internal string Name => _itemId.Name;
        internal string Version => _itemId.Version;
        internal string PositionalName => _itemId.PositionalName;

        internal static ScheduleId Create(string name, string version, string positionalName)
        {
            var timerId = Identity.New(name, version, positionalName);
            return Create(timerId);
        }
        internal static ScheduleId Create(Identity itemId, string salt="")
        {
            var identity = SwfId(itemId.Name + salt, itemId.Version, itemId.PositionalName);
            return new ScheduleId(itemId, identity);
        }

        private static string SwfId(string name, string version, string positionalName)
        {
            var combinedName = string.Format("{0}{1}{2}", name, version, positionalName).ToLower();
            return combinedName.GetMd5Hash();
        }

        internal static ScheduleId Raw(string id)
        {
            return new ScheduleId(Identity.Empty, id);
        }
        private bool Equals(ScheduleId other)
        {
            return string.Equals(_id, other._id);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ScheduleId)obj);
        }

        public static bool operator ==(ScheduleId left, ScheduleId right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left,null))
                return false;
            if (ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }
        public static bool operator !=(ScheduleId left, ScheduleId right)
        {
            return !(left == right);
        }
        public static implicit operator string(ScheduleId instance)
        {
            return instance._id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return _id;
        }
    }
}