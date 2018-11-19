// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.
namespace Guflow.Decider
{
    internal class Identity
    {
        public string Name { get; }
        public string Version { get; }
        public string PositionalName { get; }

        private Identity(string name, string version, string positionalName)
        {
            Name = name;
            Version = version;
            PositionalName = positionalName;
            Id = Decider.ScheduleId.Create(this);
        }
        //TODO: Move it out of this class
        public ScheduleId Id { get; private set; }

        public ScheduleId ScheduleId(string salt = "")
        {
            return Decider.ScheduleId.Create(this,salt);
        }
        public static readonly Identity Empty = new Identity("","","");

        public static Identity Timer(string name)
        {
            return new Identity(name,string.Empty,string.Empty);
        }

        public static Identity Lambda(string name, string positionalName = "")
        {
            return new Identity(name, string.Empty, positionalName);
        }

        public static Identity New(string name, string version, string positionalName = "")
        {
            return new Identity(name, version, positionalName);
        }

        public override bool Equals(object other)
        {
            var otherIdentity = other as Identity;
            if (otherIdentity == null)
                return false;
            return Id.Equals(otherIdentity.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{{Name {0}, Version {1} and Positional Name {2}}}", Name, Version, PositionalName);
        }
    }
}