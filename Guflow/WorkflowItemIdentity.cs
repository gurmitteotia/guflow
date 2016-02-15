namespace Guflow
{
    public class WorkflowItemIdentity
    {
        private readonly string _name;
        private readonly string _version;
        private readonly string _postionalName;

        public WorkflowItemIdentity(string name, string version, string postionalName)
        {
            _name = name;
            _version = version??string.Empty;
            _postionalName = postionalName??string.Empty;
        }

        public override bool Equals(object other)
        {
            var otherIdentity = other as WorkflowItemIdentity;
            if (otherIdentity == null)
                return false;

            return string.Equals(_name, otherIdentity._name) &&
                   string.Equals(_version, otherIdentity._version) &&
                   string.Equals(_postionalName, otherIdentity._postionalName);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}{2}", _name, _version, _postionalName).GetHashCode();
        }
    }
}