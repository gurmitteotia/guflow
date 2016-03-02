using System;
using System.Linq;
using Amazon.Auth.AccessControlPolicy;
using Guflow.Properties;

namespace Guflow
{
    public class Identity
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }
        private readonly string _id;
        private const char _idSeparator = ';';
        private Identity(string name, string version, string positionalName)
        {
            if(name.Contains(_idSeparator))
                throw new ArgumentException(string.Format(Resources.Character_not_allowed, _idSeparator), name);
            if(version.Contains(_idSeparator))
                throw new ArgumentException(string.Format(Resources.Character_not_allowed, _idSeparator), version);
            if (positionalName.Contains(_idSeparator))
                throw new ArgumentException(string.Format(Resources.Character_not_allowed, _idSeparator), positionalName);
            Name = name;
            Version = version;
            PositionalName = positionalName;
            _id = string.Format("{1}{0}{2}{0}{3}",_idSeparator, Name, Version, PositionalName);
        }
        
        internal string Id{get { return _id; }}

        internal static Identity Timer(string timerName)
        {
            return new Identity(timerName,string.Empty,string.Empty);
        }
        internal static Identity New(string name, string version, string positionalName="")
        {
            return new Identity(name, version, positionalName);
        }
        internal static Identity FromId(string id)
        {
            var splitedParts = id.Split(_idSeparator);
            if(splitedParts.Length<3)
                throw new ArgumentException(string.Format("Invalid id {0}",id));
            return new Identity(splitedParts[0],splitedParts[1],splitedParts[2]);
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