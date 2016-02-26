using System;

namespace Guflow
{
    public class Identity
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }
        private readonly string _id;
        private Identity(string name, string version, string positionalName)
        {
            Name = name;
            Version = version;
            PositionalName = positionalName;
            _id = string.Format("{0}{1}{2}", Name, Version, PositionalName);
        }
        
        internal string Id{get { return _id; }}

        public static Identity Timer(string timerName)
        {
            return new Identity(timerName,string.Empty,string.Empty);
        }

        public static Identity New(string name, string version, string positionalName="")
        {
            return new Identity(name, version, positionalName);
        }

        public override bool Equals(object other)
        {
            var otherIdentity = other as Identity;
            if (otherIdentity == null)
                return false;
            return string.Equals(_id, otherIdentity.Id, StringComparison.OrdinalIgnoreCase);
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("Name {0}, Version {1} and PositionalName {2}", Name, Version, PositionalName);
        }
    }
}