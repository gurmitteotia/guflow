// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    public sealed class AwsIdentity
    {
        private readonly string _identity;
        private AwsIdentity(string identity)
        {
            _identity = identity;
        }
        public static AwsIdentity Create(string name, string version, string positionalName)
        {
            var combinedName = string.Format("{0}{1}{2}", name, version, positionalName).ToLower();
            return new AwsIdentity(combinedName.GetMd5Hash());
        }
        public static AwsIdentity Raw(string identity)
        {
            return new AwsIdentity(identity);
        }
        private bool Equals(AwsIdentity other)
        {
            return string.Equals(_identity, other._identity);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AwsIdentity)obj);
        }

        public static bool operator ==(AwsIdentity left, AwsIdentity right)
        {
            if (ReferenceEquals(left,null))
                return false;
            return left.Equals(right);
        }
        public static bool operator !=(AwsIdentity left, AwsIdentity right)
        {
            if (ReferenceEquals(left, null))
                return false;
            return !left.Equals(right);
        }
        public static implicit operator string(AwsIdentity instance)
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