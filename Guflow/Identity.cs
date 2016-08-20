namespace Guflow
{
    internal class Identity
    {
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string PositionalName { get; private set; }
        private readonly AwsIdentity _id;
        private Identity(string name, string version, string positionalName)
        {
            Name = name;
            Version = version;
            PositionalName = positionalName;
            _id = AwsIdentity.Create(Name,Version,PositionalName);
        }
        
        internal AwsIdentity Id{get { return _id; }}

        internal static Identity Timer(string name)
        {
            return new Identity(name,string.Empty,string.Empty);
        }
        internal static Identity New(string name, string version, string positionalName="")
        {
            return new Identity(name, version, positionalName);
        }
        
        public override bool Equals(object other)
        {
            var otherIdentity = other as Identity;
            if (otherIdentity == null)
                return false;
            return _id.Equals(otherIdentity.Id);
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("Name {0}, Version {1} and Positional Name {2}", Name, Version, PositionalName);
        }
    }
}