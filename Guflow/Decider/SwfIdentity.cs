// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    public sealed class SwfIdentity
    {
        private readonly string _identity;
        private SwfIdentity(string identity)
        {
            _identity = identity;
        }
        public static SwfIdentity Create(string name, string version, string positionalName)
        {
            var combinedName = string.Format("{0}{1}{2}", name, version, positionalName).ToLower();
            return new SwfIdentity(combinedName.GetMd5Hash());
        }
        public static SwfIdentity Raw(string identity)
        {
            return new SwfIdentity(identity);
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