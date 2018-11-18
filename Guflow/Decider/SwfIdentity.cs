// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    /// <summary>
    /// Represent the identity for a scheduled workflow item. This class is used internally.
    /// </summary>
    public sealed class SwfIdentity
    {
        private readonly string _identity;
        private readonly Identity _itemId;

        private SwfIdentity(Identity itemId, string identity)
        {
            _identity = identity;
            _itemId = itemId;
        }

        internal string Name => _itemId.Name;
        internal string Version => _itemId.Version;
        internal string PositionalName => _itemId.PositionalName;

        internal static SwfIdentity Create(string name, string version, string positionalName)
        {
            var timerId = Identity.New(name, version, positionalName);
            return Create(timerId);
        }
        internal static SwfIdentity Create(Identity itemId, string salt="")
        {
            var identity = SwfId(itemId.Name + salt, itemId.Version, itemId.PositionalName);
            return new SwfIdentity(itemId, identity);
        }

        private static string SwfId(string name, string version, string positionalName)
        {
            var combinedName = string.Format("{0}{1}{2}", name, version, positionalName).ToLower();
            return combinedName.GetMd5Hash();
        }

        internal static SwfIdentity Raw(string identity)
        {
            return new SwfIdentity(Identity.Empty, identity);
        }
        private bool Equals(SwfIdentity other)
        {
            return string.Equals(_identity, other._identity);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SwfIdentity)obj);
        }

        public static bool operator ==(SwfIdentity left, SwfIdentity right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left,null))
                return false;
            if (ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }
        public static bool operator !=(SwfIdentity left, SwfIdentity right)
        {
            return !(left == right);
        }
        public static implicit operator string(SwfIdentity instance)
        {
            return instance._identity;
        }
        public override int GetHashCode()
        {
            return _identity.GetHashCode();
        }
        public override string ToString()
        {
            return _identity;
        }
    }
}